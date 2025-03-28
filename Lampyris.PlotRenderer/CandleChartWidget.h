#pragma once

#include <QWidget>
#include <QPaintEvent>
#include "QuoteCandleData.h"
#include "RenderContext.h"

class CandleChartWidget:public QWidget {
	Q_OBJECT

protected:
	void paintEvent(QPaintEvent* e) override;
public:
	CandleChartWidget(QWidget *parent);
	~CandleChartWidget();
};
