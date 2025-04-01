#pragma once

// QT Include(s)
#include <QColor>

// 配置参数结构
struct RenderConfig {
    QColor backgroundColor;
    QColor gridColor = QColor(Qt::black);

    QColor ma5Color;
    QColor ma10Color;
    QColor ma20Color;

    QColor riseColor = QColor(244,79,95); // 涨的颜色
    QColor fallColor = QColor(82,189,133); // 跌的颜色
    QColor flatColor = QColor(139,143,152); // 平的颜色
    int width;
    int height;

    // k线图网格
    int    gridColunnCount = 5;
    int    gridRowCount = 4;
    int    gridTopPadding;
};

