// Project Include(s)
#include "ControlFactory.h"

QLabel* ControlFactory::createVerticalSplitterLabel(QWidget* parent, QSize size) {
    static QPixmap originalPixmap;
    if (originalPixmap.isNull()) {
        originalPixmap = QPixmap(":/res/icons/vertical_splitter.png");
    }
    // ��̬����ü�����
    int x = originalPixmap.width() / 2 - 1; // �м䲿�ֵ� x ����
    QPixmap dividerPixmap = originalPixmap.copy(x, 0, size.width(), originalPixmap.height());

    QLabel* label = new QLabel(parent);
    label->setPixmap(dividerPixmap);
    label->setScaledContents(true); // ȷ��ͼƬ���ŵ����ʴ�С
    label->setFixedSize(size);     // ���÷ָ��ߵĿ�Ⱥ͸߶�

    return label;
}
