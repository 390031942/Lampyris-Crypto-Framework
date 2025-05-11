// Project Include(s)
#include "ControlFactory.h"

QLabel* ControlFactory::createVerticalSplitterLabel(QWidget* parent, QSize size) {
    static QPixmap originalPixmap;
    if (originalPixmap.isNull()) {
        originalPixmap = QPixmap(":/res/icons/vertical_splitter.png");
    }
    // 动态计算裁剪区域
    int x = originalPixmap.width() / 2 - 1; // 中间部分的 x 坐标
    QPixmap dividerPixmap = originalPixmap.copy(x, 0, size.width(), originalPixmap.height());

    QLabel* label = new QLabel(parent);
    label->setPixmap(dividerPixmap);
    label->setScaledContents(true); // 确保图片缩放到合适大小
    label->setFixedSize(size);     // 设置分割线的宽度和高度

    return label;
}
