#pragma once
#include <Global.h>

#if defined(LAMPYRIS_STANDALONG)

// QT Include(s)
#include <QWidget>

class MainWidget:public QWidget {
    Q_OBJECT
public:
    explicit MainWidget(QWidget* parent = Q_NULLPTR);
};

#endif // !LAMPYRIS_STANDALONG
