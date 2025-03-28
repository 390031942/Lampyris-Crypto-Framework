#pragma once

// QT Include(s)
#include <QColor>

// ���ò����ṹ
struct RenderConfig {
    QColor backgroundColor;
    QColor gridColor;

    QColor ma5Color;
    QColor ma10Color;
    QColor ma20Color;

    QColor riseColor; // �ǵ���ɫ
    QColor fallColor; // ������ɫ
    QColor flatColor; // ƽ����ɫ
    int width;
    int height;

    // k��ͼ����
    int    gridColunnCount;
    int    gridRowCount;
    int    gridTopPadding;

    QString minTick;
};

