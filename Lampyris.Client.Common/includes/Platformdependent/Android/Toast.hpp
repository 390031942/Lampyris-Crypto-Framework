#if defined(Q_OS_ANDROID)
// QT Include(s)
#include <QString>
#include <QJniObject>

class Toast {
public:
    // ��̬��������ʾ Toast ��Ϣ
    // duration: 0=��ʱ��, 1=��ʱ��
    static void showToast(const QString& message, int duration = 0) {
        // �� QString ת��Ϊ Java �� String
        QJniObject javaMessage = QJniObject::fromString(message);

        if (!javaMessage.isValid()) {
            qDebug() << "Failed to convert QString to Java String.";
            return;
        }

        // ��ȡ Android �� Context ����
        QJniObject context = QtAndroid::androidActivity();
        if (!context.isValid()) {
            qDebug() << "Failed to get Android context.";
            return;
        }

        // ���� Toast.makeText ����
        QJniObject toast = QJniObject::callStaticObjectMethod(
            "android/widget/Toast", // ����
            "makeText",             // ������
            "(Landroid/content/Context;Ljava/lang/CharSequence;I)Landroid/widget/Toast;", // ����ǩ��
            context.object(),       // Context ����
            javaMessage.object(),   // ��Ϣ����
            duration == 1 ? 1 : 0   // ��ʾʱ��: 0=��ʱ��, 1=��ʱ��
        );

        if (toast.isValid()) {
            // ��ʾ Toast
            toast.callMethod<void>("show");
        }
        else {
            qDebug() << "Failed to create Toast object.";
        }
    }
};
#endif // !Q_OS_ANDROID