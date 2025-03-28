#pragma once

#include <QWidget>
#include <QPaintEvent>

class VolumeChartWidget  : public QWidget {
	Q_OBJECT
protected:
	void paintEvent(QPaintEvent* e) override;
public:
	VolumeChartWidget(QWidget *parent);
	~VolumeChartWidget();
};
