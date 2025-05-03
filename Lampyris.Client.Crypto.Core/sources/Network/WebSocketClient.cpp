// Project Include(s)
#include "WebSocketClient.h"
#include "WebSocketMessageHandlerRegistry.h"

// QT Include(s)
#include <QDebug>
#include <QDataStream>
#include <QDateTime>

// Third-party Include(s)
#include <zlib.h>

WebSocketClient::WebSocketClient(QObject* parent) {
    connect(&m_webSocket, &QWebSocket::connected, this, &WebSocketClient::onConnected);
    connect(&m_webSocket, &QWebSocket::disconnected, this, &WebSocketClient::onDisconnected);
    connect(&m_webSocket, &QWebSocket::textMessageReceived, this, &WebSocketClient::onTextMessageReceived);
    connect(&m_webSocket, &QWebSocket::binaryMessageReceived, this, &WebSocketClient::onBinaryMessageReceived);

    // 设置重试定时器
    connect(&m_retryTimer, &QTimer::timeout, this, &WebSocketClient::onRetryConnection);

    // 设置心跳包定时器
    connect(&m_heartbeatTimer, &QTimer::timeout, this, &WebSocketClient::onSendHeartbeat);
}

WebSocketClient::~WebSocketClient() {
    m_webSocket.close();
}

void WebSocketClient::connectUrl(const QUrl& url) {
    m_url = url;
    m_retryCount = 0;  // 重置重试次数
    m_webSocket.open(m_url);
}

void WebSocketClient::sendMessage(const google::protobuf::Message& message) {
    QByteArray serializedData(message.SerializeAsString().c_str(), message.ByteSizeLong());
    QByteArray compressedData = doCompress(serializedData);
    m_webSocket.sendBinaryMessage(compressedData);
}

void WebSocketClient::sendLoginRequest(const QString& deviceMAC) {
    lampyris::crypto::protocol::app::ReqLogin reqLogin;
    reqLogin.set_devicemac(deviceMAC.toStdString());
    sendMessage(reqLogin);
}

void WebSocketClient::sendHeartbeat() {
    ReqHeartBeat reqHeartBeat;
    reqHeartBeat.set_clienttime(QDateTime::currentDateTimeUtc().toSecsSinceEpoch());
    sendMessage(reqHeartBeat);
}

void WebSocketClient::sendLogoutRequest() {
    ReqLogout reqLogout;
    sendMessage(reqLogout);
}

void WebSocketClient::onConnected() {
    qDebug() << "WebSocket connected!";
    m_retryTimer.stop();  // 停止重试定时器
    m_retryCount = 0;     // 重置重试次数

    // 启动心跳包定时器
    m_heartbeatTimer.start(m_heartbeatInterval);
}

void WebSocketClient::onDisconnected() {
    qDebug() << "WebSocket disconnected!";

    // 停止心跳包定时器
    m_heartbeatTimer.stop();

    // 启动重试定时器
    if (m_retryCount < m_maxRetries) {
        m_retryTimer.start(m_retryInterval);
    }
    else {
        qDebug() << "Max retry attempts reached. Giving up.";
    }
}

void WebSocketClient::onTextMessageReceived(const QString& message) {
    qDebug() << "Text message received:" << message;
}

void WebSocketClient::onBinaryMessageReceived(const QByteArray& message) {
    QByteArray decompressedData = doDecompress(message);

    Response response;
    if (response.ParseFromArray(decompressedData.data(), decompressedData.size())) {
        WebSocketMessageHandlerRegistry::getInstance()->invokeHandler(response);
    }
    else {
        qDebug() << "Failed to parse response!";
    }
}

void WebSocketClient::onRetryConnection() {
    qDebug() << "Retrying connection... Attempt" << (m_retryCount + 1);
    m_retryCount++;
    m_webSocket.open(m_url);
}

void WebSocketClient::onSendHeartbeat() {
    qDebug() << "Sending heartbeat...";
    sendHeartbeat();
}

QByteArray WebSocketClient::doCompress(const QByteArray& data) {
    QByteArray compressedData;
    compressedData.resize(compressBound(data.size()));

    uLongf compressedSize = compressedData.size();
    if (compress((Bytef*)compressedData.data(), &compressedSize, (const Bytef*)data.data(), data.size()) != Z_OK) {
        qDebug() << "Compression failed!";
        return QByteArray();
    }

    compressedData.resize(compressedSize);
    return compressedData;
}

QByteArray WebSocketClient::doDecompress(const QByteArray& data) {
    QByteArray decompressedData;
    decompressedData.resize(data.size() * 4);  // 预估解压后的大小

    uLongf decompressedSize = decompressedData.size();
    if (uncompress((Bytef*)decompressedData.data(), &decompressedSize, (const Bytef*)data.data(), data.size()) != Z_OK) {
        qDebug() << "Decompression failed!";
        return QByteArray();
    }

    decompressedData.resize(decompressedSize);
    return decompressedData;
}