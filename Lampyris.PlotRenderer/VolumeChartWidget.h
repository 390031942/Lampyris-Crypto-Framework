#pragma once

#include <QWidget>
#include <QPaintEvent>

class PlotRenderer;
class VolumeChartWidget  : public QWidget {
	Q_OBJECT
protected:
	void paintEvent(QPaintEvent* e) override;
public:
	VolumeChartWidget(QWidget *parent);
	~VolumeChartWidget();

	void setRenderer(PlotRenderer* renderer) {
		m_renderer = renderer;
	}

private:
	PlotRenderer* m_renderer;
};
