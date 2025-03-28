#pragma once

#include <QWidget>
#include <QPaintEvent>

#include "BinanceAPI.h"
#include <QSplitter>
#include "CandleChartWidget.h"
#include "VolumeChartWidget.h"
#include <array>

class TestWidget : public QWidget {
	Q_OBJECT
protected:
	// void paintEvent(QPaintEvent* e) override;

        // 事件过滤器
    bool eventFilter(QObject* watched, QEvent* event) override {
        if (watched == m_candleChart) {
            if (event->type() == QEvent::MouseMove) {
                QMouseEvent* mouseEvent = static_cast<QMouseEvent*>(event);
                qDebug() << "Mouse moved in child window:" << mouseEvent->pos();
            }
            else if (event->type() == QEvent::MouseButtonPress) {
                QMouseEvent* mouseEvent = static_cast<QMouseEvent*>(event);
                qDebug() << "Mouse button pressed in child window:" << mouseEvent->button();
            }
            else if (event->type() == QEvent::KeyPress) {
                QKeyEvent* keyEvent = static_cast<QKeyEvent*>(event);
                qDebug() << "Key pressed in child window:" << keyEvent->key();
            }
        }
        // 默认处理
        return QWidget::eventFilter(watched, event);
    }

private slots:
	void onDataFetched(const std::vector<QuoteCandleDataPtr>& dataList);
public:
	TestWidget(QWidget *parent);
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

    const std::array<float,13> widthArray = { 0.0625, 0.125, 0.25, 0.5, 0.7, 1, 2, 3, 4, 6, 12, 18, 24 };

    int m_widthArrayIndex = 6;

    const int MAX_LIMIT = 1500;
};
