#pragma once

#include <QWidget>
#include <QPaintEvent>

#include "BinanceAPI.h"
#include <QSplitter>
#include "CandleChartWidget.h"
#include "VolumeChartWidget.h"

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

    void handleKeyArrowLeftOrRight();

    void handleKeyArrowUpOrDown();
};
