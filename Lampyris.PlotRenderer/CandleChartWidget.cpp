#include "CandleChartWidget.h"
#include "PlotRenderer.h"

void CandleChartWidget::paintEvent(QPaintEvent* e) {
	QPainter p(this);
	m_renderer->drawCandleChart(p);
}

CandleChartWidget::CandleChartWidget(QWidget *parent)
	: QWidget(parent)
{}

CandleChartWidget::~CandleChartWidget()
{}
