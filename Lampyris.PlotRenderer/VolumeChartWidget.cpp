#include "VolumeChartWidget.h"
#include <QPainter>
#include "PlotRenderer.h"

void VolumeChartWidget::paintEvent(QPaintEvent* e) {
	QPainter p(this);
	m_renderer->drawVolume(p);
}

VolumeChartWidget::VolumeChartWidget(QWidget *parent)
	: QWidget(parent)
{}

VolumeChartWidget::~VolumeChartWidget()
{}
