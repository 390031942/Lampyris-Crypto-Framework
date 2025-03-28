#include "VolumeChartWidget.h"
#include <QPainter>

void VolumeChartWidget::paintEvent(QPaintEvent* e) {
	QPainter p(this);
	p.fillRect(rect(), Qt::blue);
}

VolumeChartWidget::VolumeChartWidget(QWidget *parent)
	: QWidget(parent)
{}

VolumeChartWidget::~VolumeChartWidget()
{}
