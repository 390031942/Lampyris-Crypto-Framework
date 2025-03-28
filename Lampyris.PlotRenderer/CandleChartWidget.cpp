#include "CandleChartWidget.h"
#include "PlotRenderer.h"

void CandleChartWidget::paintEvent(QPaintEvent* e) {
	QPainter p(this);
	p.fillRect(rect(), Qt::red);

	auto renderer = PlotRenderer();
	CandleRenderContext context;
	renderer.drawCandleChart(p);
}

CandleChartWidget::CandleChartWidget(QWidget *parent)
	: QWidget(parent)
{}

CandleChartWidget::~CandleChartWidget()
{}
