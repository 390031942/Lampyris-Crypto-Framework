#pragma once
#include <QSplitter>

class BetterSplitter : public QSplitter {
    Q_OBJECT

public:
    explicit BetterSplitter(Qt::Orientation orientation, QWidget* parent = nullptr)
        : QSplitter(orientation, parent) {}

protected:
    // 重写 createHandle 方法以自定义 QSplitterHandle
    QSplitterHandle* createHandle() override {
        QSplitterHandle* handle = QSplitter::createHandle();
        handle->setCursor((orientation() == Qt::Orientation::Vertical) ?
            Qt::CursorShape::SizeVerCursor : Qt::CursorShape::SizeHorCursor);
        return handle;
    }
};
