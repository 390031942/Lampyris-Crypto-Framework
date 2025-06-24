#pragma once

// Project Include(s)
#include <QWidget>
#include <QPaintEvent>

class PlotRenderer;
class VolumeChartWidget:public QWidget {
	Q_OBJECT
protected:
	void paintEvent(QPaintEvent* e) override;
public:
	VolumeChartWidget(QWidget *parent);
	~VolumeChartWidget();

	void setRenderer(PlotRenderer* renderer);
private:
	PlotRenderer* m_renderer;
};
