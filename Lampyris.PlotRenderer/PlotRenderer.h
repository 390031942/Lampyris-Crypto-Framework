#pragma once

// Project Include(s)
#include "RenderConfig.h"
#include "QuoteCandleData.h"
#include "MathUtil.h"
#include "RenderContext.h"

// QT Include(s)
#include <QImage>
#include <QPainter>

/**
 * 一个用于绘制K线图的工具类。
 * 该类负责绘制K线图、网格、指标文本、成交量图等内容。
 * 它使用Qt的QPainter进行绘图，并支持移动平均线（MA）等技术指标的计算和渲染。
 */
class PlotRenderer {
public:
    PlotRenderer():m_context(nullptr) {}

    /// <summary>
    /// </summary>
    /// <param name="config">渲染配置对象，包含绘图所需的配置信息。</param>
    /// <param name="dataList">K线数据列表，用于绘制图表。</param>
    PlotRenderer(const RenderConfig& config, const std::vector<QuoteCandleDataPtr>& dataList)
        : m_config(config), m_dataList(dataList), m_context(nullptr) {}

    /// <summary>
    /// 渲染主方法，负责调用各个绘图子方法。
    /// </summary>
    /// <param name="painter">QPainter对象，用于绘制图形。</param>
    void render(QPainter& painter);

    /// <summary>
    /// 绘制网格线。
    /// </summary>
    /// <param name="painter">QPainter对象，用于绘制图形。</param>
    void drawGrid(QPainter& painter);

    /// <summary>
    /// 生成移动平均线（MA）指标的字符串。
    /// </summary>
    /// <param name="period">移动平均线的周期。</param>
    /// <param name="value">移动平均线的值。</param>
    /// <returns>格式化的指标字符串。</returns>
    QString makeMAIndicatorString(int period, double value);

    /// <summary>
    /// 绘制指标文本（如移动平均线的值）。
    /// </summary>
    /// <param name="painter">QPainter对象，用于绘制图形。</param>
    void drawIndicatorText(QPainter& painter);

    /// <summary>
    /// 绘制K线图。
    /// </summary>
    /// <param name="painter">QPainter对象，用于绘制图形。</param>
    void drawCandleChart(QPainter& painter);

    /// <summary>
    /// 绘制成交量图。
    /// </summary>
    /// <param name="painter">QPainter对象，用于绘制图形。</param>
    void drawVolume(QPainter& painter);

    /// <summary>
    /// 计算移动平均线（MA）。
    /// </summary>
    /// <param name="QuoteCandleData">K线数据列表，用于计算MA。</param>
    void calculateMovingAverages(std::vector<QuoteCandleDataPtr>& QuoteCandleData);

    /// <summary>
    /// 绘制成交量的移动平均线（Volume MA）。
    /// </summary>
    /// <param name="painter">QPainter对象，用于绘制图形。</param>
    /// <param name="ma">移动平均线数据。</param>
    /// <param name="color">移动平均线的颜色。</param>
    /// <param name="volumeTop">成交量图的顶部位置。</param>
    /// <param name="volumeHeight">成交量图的高度。</param>
    /// <param name="maxVolume">成交量的最大值，用于归一化。</param>
    void drawVolumeMA(QPainter& painter,
                      const std::vector<double>& ma,
                      const QColor& color,
                      int volumeTop,
                      int volumeHeight,
                      double maxVolume);

    /// <summary>
    /// 绘制价格标记（如最高价和最低价）。
    /// </summary>
    /// <param name="painter">QPainter对象，用于绘制图形。</param>
    /// <param name="index">当前K线的索引。</param>
    /// <param name="price">标记的价格值。</param>
    /// <param name="gridMinPrice">网格的最小价格。</param>
    /// <param name="isMax">是否为最高价标记。</param>
    /// <param name="klineAreaHeight">K线图区域的高度。</param>
    /// <param name="priceRange">价格范围（最高价 - 最低价）。</param>
    /// <param name="minPrice">最低价格。</param>
    /// <param name="candleWidth">K线的宽度。</param>
    /// <param name="padding">绘制时的边距。</param>
    void drawPriceMarker(QPainter& painter,
                         int index,
                         double price,
                         double gridMinPrice,
                         bool isMax,
                         int klineAreaHeight,
                         double priceRange,
                         double minPrice,
                         int candleWidth,
                         int padding);

    /// <summary>
    /// 绘制K线图的移动平均线（KLine MA）。
    /// </summary>
    /// <param name="painter">QPainter对象，用于绘制图形。</param>
    /// <param name="fieldOffset">数据字段的偏移量（如开盘价、收盘价）。</param>
    /// <param name="color">移动平均线的颜色。</param>
    /// <param name="klineHeight">K线图区域的高度。</param>
    /// <param name="priceRange">价格范围（最高价 - 最低价）。</param>
    /// <param name="minPrice">最低价格。</param>
    /// <param name="candleWidth">K线的宽度。</param>
    void drawKLineMA(QPainter& painter,
                     int fieldOffset,
                     const QColor& color,
                     int klineHeight,
                     double priceRange,
                     double minPrice,
                     int candleWidth);

    /// <summary>
    /// 设置渲染上下文。
    /// </summary>
    /// <param name="context">渲染上下文对象，包含绘图所需的动态参数。</param>
    void setRenderContext(CandleRenderContext* context) { m_context = context; }
private:
    /// <summary>
    /// 渲染配置,包括颜色等
    /// </summary>
    RenderConfig  m_config; 
    std::vector<QuoteCandleDataPtr> m_dataList;      ///< K线数据列表。
    CandleRenderContext* m_context;       ///< 渲染上下文对象。
};
