#pragma once
#include <Global.h>

#if defined(LAMPYRIS_STANDALONG)

// QT Include(s)
#include <QObject>
#include <QWidget>

class MainStatusBar : public QWidget {
    Q_OBJECT
public:
    explicit MainStatusBar(QWidget *parent = nullptr);
};

#endif // !LAMPYRIS_STANDALONG
