#pragma once

// QT Include(s)
#include <QColor>

// ���ò����ṹ
struct RenderConfig {
    QColor backgroundColor;
    QColor gridColor = QColor(Qt::black);

    QColor ma5Color;
    QColor ma10Color;
    QColor ma20Color;

    QColor riseColor = QColor(244,79,95); // �ǵ���ɫ
    QColor fallColor = QColor(82,189,133); // ������ɫ
    QColor flatColor = QColor(139,143,152); // ƽ����ɫ
    int width;
    int height;

    // k��ͼ����
    int    gridColunnCount = 5;
    int    gridRowCount = 4;
    int    gridTopPadding;
};

