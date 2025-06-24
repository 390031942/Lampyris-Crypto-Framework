#include "BetterSplitter.h"

QSplitterHandle* BetterSplitter::createHandle() {
    QSplitterHandle* handle = QSplitter::createHandle();
    handle->setCursor((orientation() == Qt::Orientation::Vertical) ?
        Qt::CursorShape::SizeVerCursor : Qt::CursorShape::SizeHorCursor);
    return handle;
}
