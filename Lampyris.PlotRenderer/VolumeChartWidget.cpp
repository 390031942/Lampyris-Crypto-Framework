// Project Include(s)
#include "VolumeChartWidget.h"
#include "PlotRenderer.h"

// QT Include(s)
#include <QPainter>

void VolumeChartWidget::paintEvent(QPaintEvent* e) {
	QPainter p(this);
	m_renderer->drawVolume(p);
}

VolumeChartWidget::VolumeChartWidget(QWidget *parent)
	: QWidget(parent), m_renderer(Q_NULLPTR)
{}

VolumeChartWidget::~VolumeChartWidget()
{}

void VolumeChartWidget::setRenderer(PlotRenderer * renderer) {
	m_renderer = renderer;
}
