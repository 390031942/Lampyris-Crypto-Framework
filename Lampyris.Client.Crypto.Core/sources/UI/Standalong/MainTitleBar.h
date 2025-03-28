#pragma once
#include <Global.h>

#if defined(LAMPYRIS_STANDALONG)

// QT Include(s)
#include <QObject>
#include <QWidget>

class MainTitleBar : public QWidget {
    Q_OBJECT
public:
    explicit MainTitleBar(QWidget *parent = nullptr);
};

#endif // !LAMPYRIS_STANDALONG
