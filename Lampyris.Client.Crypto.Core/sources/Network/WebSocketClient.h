#pragma once

// QT Include(s)
#include <QObject>
#include <QWebSocket>
#include <QByteArray>
#include <QTimer>

// Project Include(s)
#include <Base/Singleton.h>
#include "Protocol/Protocols.h"

// WebSocketClient ��
// ������� WebSocket ���ӡ���Ϣ���ͺͽ���
class WebSocketClient : public SingletonQObject<WebSocketClient> {
    Q_OBJECT
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

#pragma region ����ѹ�����
    QByteArray doCompress(const QByteArray& data);
    QByteArray doDecompress(const QByteArray& data);
#pragma endregion

#pragma region ���Ի������
    // ���Լ�������룩
    int        m_retryInterval = 5000;  
    // ������Դ���
    int        m_maxRetries = 5;     
    // ��ǰ���Դ���
    int        m_retryCount = 0;        
    // ���Զ�ʱ��
    QTimer     m_retryTimer;            
#pragma endregion

#pragma region �������������
    // ��������ʱ��
    QTimer     m_heartbeatTimer;        
    // ���������ͼ�������룩
    int        m_heartbeatInterval = 30000;  
#pragma endregion
};
