#if defined(Q_OS_ANDROID)
// QT Include(s)
#include <QString>
#include <QJniObject>

class Toast {
public:
    // 静态方法：显示 Toast 消息
    // duration: 0=短时间, 1=长时间
    static void showToast(const QString& message, int duration = 0) {
        // 将 QString 转换为 Java 的 String
        QJniObject javaMessage = QJniObject::fromString(message);

        if (!javaMessage.isValid()) {
            qDebug() << "Failed to convert QString to Java String.";
            return;
        }

        // 获取 Android 的 Context 对象
        QJniObject context = QtAndroid::androidActivity();
        if (!context.isValid()) {
            qDebug() << "Failed to get Android context.";
            return;
        }

        // 调用 Toast.makeText 方法
        QJniObject toast = QJniObject::callStaticObjectMethod(
            "android/widget/Toast", // 类名
            "makeText",             // 方法名
            "(Landroid/content/Context;Ljava/lang/CharSequence;I)Landroid/widget/Toast;", // 方法签名
            context.object(),       // Context 参数
            javaMessage.object(),   // 消息内容
            duration == 1 ? 1 : 0   // 显示时间: 0=短时间, 1=长时间
        );

        if (toast.isValid()) {
            // 显示 Toast
            toast.callMethod<void>("show");
        }
        else {
            qDebug() << "Failed to create Toast object.";
        }
    }
};
#endif // !Q_OS_ANDROID