#pragma once

// QT Include(s)
#include <QObject>
#include <QWebSocket>
#include <QByteArray>
#include <QTimer>

// Project Include(s)
#include <Base/Singleton.h>
#include "Protocol/Protocols.h"

// WebSocketClient 类
// 负责管理 WebSocket 连接、消息发送和接收
class WebSocketClient:public QObject {
    Q_OBJECT
    DECLARE_SINGLETON_QOBJECT(WebSocketClient)
public:
    explicit   WebSocketClient(QObject* parent = nullptr);
              ~WebSocketClient();
    void       connectUrl(const QUrl& url);
    void       sendMessage(const google::protobuf::Message& message);
    void       sendLoginRequest(const QString& deviceMAC);
    void       sendHeartbeat();
    void       sendLogoutRequest();
signals:
    void       messageReceived(const QString& message);
private slots:
    void       onConnected();
    void       onDisconnected();
    void       onTextMessageReceived(const QString& message);
    void       onBinaryMessageReceived(const QByteArray& message);
    void       onRetryConnection();
    void       onSendHeartbeat();
private:
    QWebSocket m_webSocket;
    QUrl       m_url;

#pragma region 数据压缩相关
    QByteArray doCompress(const QByteArray& data);
    QByteArray doDecompress(const QByteArray& data);
#pragma endregion

#pragma region 重试机制相关
    // 重试间隔（毫秒）
    int        m_retryInterval = 5000;  
    // 最大重试次数
    int        m_maxRetries = 5;     
    // 当前重试次数
    int        m_retryCount = 0;        
    // 重试定时器
    QTimer     m_retryTimer;            
#pragma endregion

#pragma region 心跳包机制相关
    // 心跳包定时器
    QTimer     m_heartbeatTimer;        
    // 心跳包发送间隔（毫秒）
    int        m_heartbeatInterval = 30000;  
#pragma endregion
};
