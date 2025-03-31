#pragma once

#include <QWidget>
#include <QPaintEvent>

#include "BinanceAPI.h"
#include <QSplitter>
#include "CandleChartWidget.h"
#include "VolumeChartWidget.h"
#include <array>
#include "BetterSplitter.h"

class TestWidget : public QWidget {
    Q_OBJECT
protected:
    // void paintEvent(QPaintEvent* e) override;

        // ÊÂ¼þ¹ýÂËÆ÷
    bool eventFilter(QObject* watched, QEvent* event) override;

    void resizeEvent(QResizeEvent* e);

    void keyPressEvent(QKeyEvent* e) override;

    void mouseMoveEvent(QMouseEvent* e) override;

    void handleMouseMove(QMouseEvent* e);

    int calculateFocusIndex(double X, double leftOffset, double width, double spacing, int numKlines);

    int calculateCandleCount(double leftOffset, double width, double spacing, double windowWidth);

private slots:
    void onDataFetched(const std::vector<QuoteCandleDataPtr>& dataList);
public:
    TestWidget(QWidget* parent);
    ~TestWidget();
private:
    BinanceAPI api;

    CandleChartWidget* m_candleChart;
    VolumeChartWidget* m_volumeChart;

    std::vector<QuoteCandleDataPtr> m_fullDataList;

    CandleRenderContext m_context;

    void handleMouseMove(QPoint mousePos);

    void handleKeyArrowLeftOrRight(int key);

    void handleKeyArrowUpOrDown(int key);

    void recalculateContextParam();

    const std::array<double, 13> widthArray = { 0.0625, 0.125, 0.25, 0.5, 0.7, 1, 2, 3, 4, 6, 12, 18, 24 };

    int m_widthArrayIndex = 9;

    bool m_isFirstTime = true;

    void reset();

    const int MAX_LIMIT = 1500;

    BetterSplitter* m_betterSplitter;

    void repaintChart() {
        this->m_candleChart->repaint();
        this->m_volumeChart->repaint();
    }
};
