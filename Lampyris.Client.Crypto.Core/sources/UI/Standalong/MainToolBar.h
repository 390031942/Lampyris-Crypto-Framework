#pragma once
#include <Global.h>

#if defined(LAMPYRIS_STANDALONG)

// QT Include(s)
#include <QObject>
#include <QWidget>

class MainToolBar : public QWidget {
    Q_OBJECT
public:
    explicit MainToolBar(QWidget *parent = nullptr);
};

#endif // !LAMPYRIS_STANDALONG
