#pragma once

// Project Include(s)
#include "RenderConfig.h"
#include "QuoteCandleData.h"
#include "MathUtil.h"
#include "RenderContext.h"

// QT Include(s)
#include <QImage>
#include <QPainter>

class PlotRenderer {
public:
    PlotRenderer() {}
    PlotRenderer(const RenderConfig& config, 
                 const std::vector<QuoteCandleDataPtr>& dataList)
        : m_config(config), m_dataList(dataList) {}
                                    
    void                            render(QPainter& painter);
                                    
    void                            drawGrid(QPainter& painter, 
                                             double maxPrice, 
                                             double minPrice,
                                             double gridMaxPrice, 
                                             double gridMinPrice);
                                    
    QString                         makeMAIndicatorString(int period,
                                                          double value);
                                    
    void                            drawIndicatorText(QPainter& painter);
                                    
    void                            drawCandleChart(QPainter& painter);
                                    
    void                            drawVolume(QPainter& painter);
                                    
    void                            calculateMovingAverages(std::vector<QuoteCandleDataPtr>& QuoteCandleData);
                                    
    void                            drawVolumeMA(QPainter& painter, 
                                                 const std::vector<double>& ma, 
                                                 const QColor& color,
                                                 int volumeTop, 
                                                 int volumeHeight,
                                                 double maxVolume);
                                    
    void                            drawPriceMarker(QPainter& painter, 
                                                    int index,
                                                    double price,
                                                    double gridMinPrice,
                                                    bool isMax,
                                                    int klineAreaHeight,
                                                    double priceRange,
                                                    double minPrice,
                                                    int candleWidth,
                                                    int padding);
                                    
    void                            drawKLineMA(QPainter& painter,
                                                int fieldOffset, 
                                                const QColor& color, 
                                                int klineHeight,
                                                double priceRange,
                                                double minPrice,
                                                int candleWidth);

    void                            setRenderContext(CandleRenderContext* context) 
    { m_context = context;}
private:
    RenderConfig                    m_config;
    std::vector<QuoteCandleDataPtr> m_dataList;
    CandleRenderContext*            m_context;
};                                  