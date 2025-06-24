#pragma once

// Project Include(s)
#include "GridChartWidget.h"

// QT Include(s)
#include <QPoint>
#include <QString>

class BetterCandleChartWidget : public GridChartWidget {
    Q_OBJECT
public:
    explicit BetterCandleChartWidget(QWidget* parent = 0);

    bool     readData(QString strFile);
    void     initialize();
    void     drawLine();
    void     getIndicator();
    void     drawYtick();
    void     drawKline();

    // 键盘按下后画的十字线
    void     drawCross();
    void     drawCrossVerLine();
    void     drawCrossHorLine();
    void     drawTips();

    // 键盘没按下画的十字线
    void     drawCross2();
    void     drawTips2();

    // 画均线
    void     drawAverageLine(int day);
protected:
    virtual void paintEvent(QPaintEvent* event) override;
    virtual void keyPressEvent(QKeyEvent* event) override;
    virtual void resizeEvent(QResizeEvent* event) override;
private:
    // 画 K 线的起始日期和终止日期
    int      m_beginDay;
    int      m_endDay;
    int      m_totalDay;
    int      m_currentDay;

    // 当前要画的 K 线日期中的最高价、最低价、最大成交量
    double   m_highestBid;
    double   m_lowestBid;
    double   m_maxVolume;

    // x 轴和 y 轴的缩放比
    double   m_xScale;
    double   m_yScale;

    // 是否显示十字线
    bool     m_bCross = false;

    // 画笔的线宽
    int      m_lineWidth;

    // 键盘是否按下
    bool     m_isKeyDown = false;

    // 是否画均线
    bool     m_isDrawAverageLine = true;
};