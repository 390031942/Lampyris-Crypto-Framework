#pragma once

// QT Include(s)
#include <QColor>

// 配置参数结构
struct RenderConfig {
    QColor backgroundColor;
    QColor gridColor;

    QColor ma5Color;
    QColor ma10Color;
    QColor ma20Color;

    QColor riseColor; // 涨的颜色
    QColor fallColor; // 跌的颜色
    QColor flatColor; // 平的颜色
    int width;
    int height;

    // k线图网格
    int    gridColunnCount;
    int    gridRowCount;
    int    gridTopPadding;

    QString minTick;
};

