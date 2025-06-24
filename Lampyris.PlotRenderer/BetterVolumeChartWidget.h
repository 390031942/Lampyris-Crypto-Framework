#pragma once

// Project Include(s)
#include "GridChartWidget.h"

// QT Include(s)
#include <QPoint>

class BetterVolumeChartWidget : public GridChartWidget {
public:
    explicit BetterVolumeChartWidget(QWidget* parent);
    bool readData(QString strFile);
    void initialize();
    void drawYtick();
    void drawVolume();
    void getIndicator();
    void drawAverageLine(int day);

protected:
    virtual void paintEvent(QPaintEvent* event) override;
private:
    //画成交量线的起始日期和终止日期
    int beginDay;
    int endDay;
    int totalDay;
    int currentDay;
    //当前要画的成交量线中的最大成交量
    double maxVolume;
    //鼠标位置
    QPoint mousePoint;
    //线宽
    int lineWidth;
};
