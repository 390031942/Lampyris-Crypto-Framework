#pragma once

// QT Include(s)
#include <QWidget>
#include <QPaintEvent>

// STD Include(s)
#include <array>

// Project Include(s)
#include "BinanceAPI.h"
#include "CandleChartWidget.h"
#include "VolumeChartWidget.h"
#include "BetterSplitter.h"

class CandleQuoteDisplayWidget : public QWidget {
    Q_OBJECT

   // K线宽度列表
   const static std::array<double, 13> WIDTH_ARRAY;
public:
            CandleQuoteDisplayWidget(QWidget* parent);
           ~CandleQuoteDisplayWidget();
protected:
    bool    eventFilter(QObject* watched, QEvent* event) override;
    void    resizeEvent(QResizeEvent* e) override;
    void    keyPressEvent(QKeyEvent* e) override;
    void    mouseMoveEvent(QMouseEvent* e) override;
private slots:
    void    onDataFetched(const std::vector<QuoteCandleDataPtr>& dataList);
private:
    void    handleMouseMove(QMouseEvent* e);
    void    handleMouseMove(QPoint mousePos);
    void    handleKeyArrowLeftOrRight(int key);
    void    handleKeyArrowUpOrDown(int key);
    void    recalculateContextParam();
    QString getMinTick(const QString& symbol);
    QString removeTrailingZeros(const QString& numberStr);
    void    reset();
    void    repaintChart();
    int     calculateFocusIndex(double X, double leftOffset, double width, double spacing, int numKlines);
    int     calculateCandleCount(double leftOffset, double width, double spacing, double windowWidth);

    const int                       MAX_LIMIT = 1500;
    BinanceAPI                      api;
    CandleChartWidget*              m_candleChart;
    VolumeChartWidget*              m_volumeChart;
    std::vector<QuoteCandleDataPtr> m_fullDataList;
    CandleRenderContext             m_context;
    int                             m_widthArrayIndex = 9;
    bool                            m_isFirstTime = true;
    BetterSplitter*                 m_betterSplitter;
};
