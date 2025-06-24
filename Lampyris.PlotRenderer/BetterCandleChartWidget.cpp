#include "BetterCandleChartWidget.h"
#include <QMessageBox>
#include <QPainter>
#include <QPen>
#include <QKeyEvent>
#include <QMouseEvent>
#include <QVector>
#include <QDockWidget>
#include <QWidget>
#include "mainwindow.h"

BetterCandleChartWidget::BetterCandleChartWidget(QWidget* parent) : GridChartWidget(parent) {
    // 开启鼠标追踪
    setMouseTracking(true);

    initialize();
}

bool BetterCandleChartWidget::readData(QString strFile) {
    return m_dataFile.readData(strFile);
}

void BetterCandleChartWidget::initialize() {
    // 读取数据
    QString file = tr("dataKLine.txt");
    if (!m_dataFile.readData(file)) {
        QMessageBox::about(this, tr("数据文件读取失败"), tr("确定"));
        return;
    }

    // 开启鼠标追踪
    setMouseTracking(true);

    // 初始化一些成员变量
    m_endDay = m_dataFile.kline.size() - 1;
    m_totalDay = 200;
    m_beginDay = m_endDay - m_totalDay;
    m_currentDay = m_beginDay + m_totalDay / 2;

    if (m_beginDay < 0) {
        m_beginDay = 0;
        m_totalDay = m_dataFile.kline.size();
    }

    m_highestBid = 0;
    m_lowestBid = 1000;
    m_maxVolume = 0;
}

void BetterCandleChartWidget::paintEvent(QPaintEvent* event) {
    GridChartWidget::paintEvent(event);

    // 画 K 线
    drawLine();
}

void BetterCandleChartWidget::drawLine() {
    // 获取 y 轴指标
    getIndicator();

    // 显示 y 轴价格
    drawYtick();

    // 画 K 线
    drawKline();

    // 画十字线
    if (!m_isKeyDown && m_bCross) {
        drawCross2();
    }

    if (m_isKeyDown && m_bCross) {
        drawCross();
    }

    // 画均线
    drawAverageLine(5);
    drawAverageLine(10);
    drawAverageLine(20);
    drawAverageLine(30);
    drawAverageLine(60);
}

void BetterCandleChartWidget::getIndicator() {
    m_highestBid = 0;
    m_lowestBid = 1000;
    m_maxVolume = 0;

    for (int i = m_beginDay; i < m_endDay; ++i) {
        if (m_dataFile.kline[i].highestBid > m_highestBid)
            m_highestBid = m_dataFile.kline[i].highestBid;
        if (m_dataFile.kline[i].lowestBid < m_lowestBid)
            m_lowestBid = m_dataFile.kline[i].lowestBid;
    }
}

void BetterCandleChartWidget::drawYtick() {
    QPainter painter(this);
    QPen pen;
    pen.setColor(Qt::white);
    painter.setPen(pen);

    double yStep = (m_highestBid - m_lowestBid) / getHorizontalGridCount();
    QString str;

    auto metrics = painter.fontMetrics();
    QString lowestPricerStr = str.sprintf("%.2f", m_lowestBid);
    QString highestPricerStr = str.sprintf("%.2f", m_highestBid);

    auto advance = std::max(metrics.horizontalAdvance(lowestPricerStr),
                            metrics.horizontalAdvance(highestPricerStr));

    auto tickTextStartX = getWidgetWidth() - 3 - advance;
    auto height = metrics.height();
    setMarginRight(6 + advance);

    painter.drawText(QPoint(tickTextStartX, getMarginTop() + height), highestPricerStr);
    painter.drawText(QPoint(tickTextStartX, getWidgetHeight() - getMarginBottom()), lowestPricerStr);

    if (getHorizontalGridCount() == 0) {
        return;
    }

    for (int i = 0; i <= getHorizontalGridCount(); ++i) {
        str.sprintf("%.2f", m_lowestBid + i * yStep);
        painter.drawText(QPoint(tickTextStartX,
            getWidgetHeight() - getMarginBottom() - i * getCellGridHeight() + height / 2),
            str);
    }
}

void BetterCandleChartWidget::drawKline() {
    QPainter painter(this);
    QPen pen;
    pen.setColor(Qt::red);
    painter.setPen(pen);

    QBrush brush;

    if (m_beginDay < 0)
        return;

    // y 轴缩放
    m_yScale = getGridHeight() / (m_highestBid - m_lowestBid);

    // 画笔的线宽
    m_lineWidth;

    // 画线连接的两个点
    QPoint p1;
    QPoint p2;

    QPoint p3;
    QPoint p4;

    double xStep = getGridWidth() / m_totalDay;

    for (int i = m_beginDay; i < m_endDay; ++i) {
        if (m_dataFile.kline[i].openingPrice <= m_dataFile.kline[i].closeingPrice) {
            pen.setColor(QColor(246, 70, 93));
            brush.setColor(QColor(246, 70, 93));
        }
        else {
            pen.setColor(QColor(46, 189, 133));
            brush.setColor(QColor(46, 189, 133));
        }

        m_lineWidth = getGridWidth() / m_totalDay;

        // 为了各个 K 线之间不贴在一起，设置一个间隔
        m_lineWidth = m_lineWidth - 0.2 * m_lineWidth;

        // 最小线宽为 3
        if (m_lineWidth < 3)
            m_lineWidth = 3;

        // 画开盘与收盘之间的粗实线
        pen.setWidth(m_lineWidth);
        painter.setPen(pen);
        painter.setBrush(brush);

        p1.setX(getMarginLeft() + xStep * (i - m_beginDay) + 0.5 * m_lineWidth);
        p1.setY(getWidgetHeight() - (m_dataFile.kline[i].openingPrice - m_lowestBid) * m_yScale - getMarginBottom());
        p2.setX(getMarginLeft() + xStep * (i - m_beginDay) + 0.5 * m_lineWidth);
        p2.setY(getWidgetHeight() - (m_dataFile.kline[i].closeingPrice - m_lowestBid) * m_yScale - getMarginBottom() - 0.5 * m_lineWidth);
        painter.drawLine(p1, p2);

        // 画最高价与最低价之间的细线
        pen.setWidth(1);
        painter.setPen(pen);
        painter.setBrush(Qt::transparent);
        p1.setX(getMarginLeft() + xStep * (i - m_beginDay) + 0.5 * m_lineWidth);
        p1.setY(getWidgetHeight() - (m_dataFile.kline[i].highestBid - m_lowestBid) * m_yScale - getMarginBottom());
        p2.setX(getMarginLeft() + xStep * (i - m_beginDay) + 0.5 * m_lineWidth);
        p2.setY(getWidgetHeight() - (m_dataFile.kline[i].lowestBid - m_lowestBid) * m_yScale - getMarginBottom());
        painter.drawLine(p1, p2);
    }
}

void BetterCandleChartWidget::keyPressEvent(QKeyEvent* event) {
    m_currentDay = (double)(m_mousePoint.x() - getMarginLeft()) / (getGridWidth()) * m_totalDay + m_beginDay;

    m_isKeyDown = true;
    switch (event->key()) {
    case Qt::Key_Left:
    {
        double xstep = getGridWidth() / m_totalDay;

        if (m_mousePoint.x() - xstep < getMarginLeft()) {
            if (m_beginDay - 1 < 0)
                return;
            m_endDay -= 1;
            m_beginDay -= 1;
        }
        else
            m_mousePoint.setX(m_mousePoint.x() - xstep);

        update();
        break;
    }

    case Qt::Key_Right:
    {
        double xstep = getGridWidth() / m_totalDay;

        if (m_mousePoint.x() + xstep > getWidgetWidth() - getMarginRight()) {
            if (m_endDay + 1 > m_dataFile.kline.size() - 1)
                return;
            m_endDay += 1;
            m_beginDay += 1;
        }
        else
            m_mousePoint.setX(m_mousePoint.x() + xstep);


        update();
        break;
    }

    case Qt::Key_Up:
    {
        m_totalDay = m_totalDay / 2;

        //最少显示10个
        if (m_totalDay < 10) {
            m_totalDay *= 2;
            return;
        }


        m_endDay = m_currentDay + m_totalDay / 2;
        m_beginDay = m_currentDay - m_totalDay / 2;

        if (m_endDay > m_dataFile.kline.size() - 10) {
            m_endDay = m_dataFile.kline.size() - 10;
            m_beginDay = m_endDay - m_totalDay;
        }

        if (m_beginDay < 0) {
            m_beginDay = 0;
            m_endDay = m_beginDay + m_totalDay;
        }

        update();


        break;
    }

    case Qt::Key_Down:
    {
        if (m_totalDay == m_dataFile.kline.size() - 1)
            return;

        m_totalDay = m_totalDay * 2;
        if (m_totalDay > m_dataFile.kline.size() - 1) {
            m_totalDay = m_dataFile.kline.size() - 1;
        }


        m_endDay = m_currentDay + m_totalDay / 2;
        if (m_endDay > m_dataFile.kline.size() - 10) {
            m_endDay = m_dataFile.kline.size() - 10;
        }

        m_beginDay = m_currentDay - m_totalDay / 2;
        if (m_beginDay < 0)
            m_beginDay = 0;

        m_totalDay = m_endDay - m_beginDay;

        update();

    }
    default:
        break;
    }
}

void BetterCandleChartWidget::resizeEvent(QResizeEvent* event) {
    GridChartWidget::resizeEvent(event); // 这里原代码用的AutoGrid，推测应该是GridChartWidget，可按需调整
    m_bCross = false;
}

void BetterCandleChartWidget::drawCross() {
    drawCrossVerLine();
    drawCrossHorLine();
    drawTips();
}

void BetterCandleChartWidget::drawCrossVerLine() {
    QPainter painter(this);
    QPen pen;
    pen.setColor(QColor("#FFFFFF"));
    pen.setWidth(1);
    painter.setPen(pen);

    double xstep = getGridWidth() / m_totalDay;
    double xPos = getMarginLeft();
    while (m_mousePoint.x() - xPos > xstep) {
        xPos += xstep;
    }
    xPos += 0.5 * m_lineWidth;
    QLine horline(xPos, getMarginTop(), xPos, getWidgetHeight() - getMarginBottom());
    painter.drawLine(horline);
}

void BetterCandleChartWidget::drawCrossHorLine() {
    QPainter painter(this);
    QPen pen;
    pen.setColor(QColor("#FFFFFF"));
    pen.setWidth(1);
    painter.setPen(pen);

    double yPos;
    m_currentDay = (m_mousePoint.x() - getMarginLeft()) * m_totalDay / getGridWidth() + m_beginDay;

    if (m_dataFile.kline[m_currentDay].openingPrice < m_dataFile.kline[m_currentDay].closeingPrice)
        yPos = (m_dataFile.kline[m_currentDay].closeingPrice - m_lowestBid) * m_yScale;
    else
        yPos = (m_dataFile.kline[m_currentDay].closeingPrice - m_lowestBid) * m_yScale;

    QLine verline(getMarginLeft(), getWidgetHeight() - getMarginBottom() - yPos,
        getWidgetWidth() - getMarginRight(), getWidgetHeight() - getMarginBottom() - yPos);
    painter.drawLine(verline);
}

void BetterCandleChartWidget::drawTips() {
    QPainter painter(this);
    QPen pen;
    QBrush brush(QColor(64, 0, 128));
    painter.setBrush(brush);
    pen.setColor(QColor("#FFFFFF"));
    pen.setWidth(1);
    painter.setPen(pen);

    int currentDay = (m_mousePoint.x() - getMarginLeft()) * m_totalDay / getGridWidth() + m_beginDay;
    double yval = m_dataFile.kline[currentDay].closeingPrice;

    double yPos;
    if (m_dataFile.kline[currentDay].openingPrice < m_dataFile.kline[currentDay].closeingPrice)
        yPos = (m_dataFile.kline[currentDay].closeingPrice - m_lowestBid) * m_yScale;
    else
        yPos = (m_dataFile.kline[currentDay].closeingPrice - m_lowestBid) * m_yScale;

    yPos = getWidgetHeight() - getMarginBottom() - yPos;

    int iTipsWidth = 60;
    int iTipsHeight = 30;

    QString str;

    QRect rect(getWidgetWidth() - getMarginRight(),
        yPos - iTipsHeight / 2, iTipsWidth, iTipsHeight);
    painter.drawRect(rect);

    QRect rectText(getWidgetWidth() - getMarginRight() + iTipsWidth / 4,
        yPos - iTipsHeight / 4, iTipsWidth, iTipsHeight);
    painter.drawText(rectText, str.sprintf("%.2f", yval));

    if (currentDay == 0)
        return;

    QColor openingColor = m_dataFile.kline[currentDay].openingPrice > m_dataFile.kline[currentDay - 1].openingPrice ?
        QColor("#FF0000") : QColor("#00FF00");

    QColor highestColor = m_dataFile.kline[currentDay].highestBid > m_dataFile.kline[currentDay - 1].closeingPrice ?
        QColor("#FF0000") : QColor("#00FF00");

    QColor lowestColor = m_dataFile.kline[currentDay].lowestBid > m_dataFile.kline[currentDay - 1].closeingPrice ?
        QColor("#FF0000") : QColor("#00FF00");

    QColor closeingColor = m_dataFile.kline[currentDay].closeingPrice > m_dataFile.kline[currentDay].openingPrice ?
        QColor("#FF0000") : QColor("#00FF00");

    QColor amountOfIncreaseColor = m_dataFile.kline[currentDay].amountOfIncrease > 0 ?
        QColor("#FF0000") : QColor("#00FF00");
}

void BetterCandleChartWidget::drawCross2() {
    drawMouseMoveCrossHorLine();
    drawMouseMoveCrossVerLine();
    drawTips2();
}

void BetterCandleChartWidget::drawTips2() {
    QPainter painter(this);
    QPen pen;
    QBrush brush(QColor(64, 0, 128));
    painter.setBrush(brush);
    pen.setColor(QColor("#FFFFFF"));
    pen.setWidth(1);
    painter.setPen(pen);

    double yval = m_highestBid - (m_mousePoint.y() - getMarginTop()) / m_yScale;
    double yPos = m_mousePoint.y();

    int iTipsWidth = 60;
    int iTipsHeight = 30;

    QString str;

    QRect rect(getWidgetWidth() - getMarginRight(),
        yPos - iTipsHeight / 2, iTipsWidth, iTipsHeight);
    painter.drawRect(rect);

    QRect rectText(getWidgetWidth() - getMarginRight() + iTipsWidth / 4,
        yPos - iTipsHeight / 4, iTipsWidth, iTipsHeight);
    painter.drawText(rectText, str.sprintf("%.2f", yval));
}

void BetterCandleChartWidget::drawAverageLine(int day) {
    // y 轴缩放
    m_yScale = getGridHeight() / (m_highestBid - m_lowestBid);
    // 画笔的线宽
    m_lineWidth;
    // 画线要连接的点
    QVector<QPoint> point;

    // 临时点
    QPoint temp;

    // x 轴步进
    double xstep = getGridWidth() / m_totalDay;

    if (m_beginDay < 0)
        return;

    switch (day) {
    case 5:
        for (int i = m_beginDay; i < m_endDay; ++i) {
            if (m_dataFile.kline[i].averageLine5 == 0)
                continue;
            temp.setX(getMarginLeft() + xstep * (i - m_beginDay) + 0.5 * m_lineWidth);
            temp.setY(getWidgetHeight() - (m_dataFile.kline[i].averageLine5 - m_lowestBid) * m_yScale - getMarginBottom());
            point.push_back(temp);
        }
        break;
    case 10:
        for (int i = m_beginDay; i < m_endDay; ++i) {
            if (m_dataFile.kline[i].averageLine10 == 0)
                continue;
            temp.setX(getMarginLeft() + xstep * (i - m_beginDay) + 0.5 * m_lineWidth);
            temp.setY(getWidgetHeight() - (m_dataFile.kline[i].averageLine10 - m_lowestBid) * m_yScale - getMarginBottom());
            point.push_back(temp);
        }
        break;
    case 20:
        for (int i = m_beginDay; i < m_endDay; ++i) {
            if (m_dataFile.kline[i].averageLine20 == 0)
                continue;
            temp.setX(getMarginLeft() + xstep * (i - m_beginDay) + 0.5 * m_lineWidth);
            temp.setY(getWidgetHeight() - (m_dataFile.kline[i].averageLine20 - m_lowestBid) * m_yScale - getMarginBottom());
            point.push_back(temp);
        }
        break;
    case 30:
        for (int i = m_beginDay; i < m_endDay; ++i) {
            if (m_dataFile.kline[i].averageLine30 == 0)
                continue;
            temp.setX(getMarginLeft() + xstep * (i - m_beginDay) + 0.5 * m_lineWidth);
            temp.setY(getWidgetHeight() - (m_dataFile.kline[i].averageLine30 - m_lowestBid) * m_yScale - getMarginBottom());
            point.push_back(temp);
        }
        break;
    case 60:
        for (int i = m_beginDay; i < m_endDay; ++i) {
            if (m_dataFile.kline[i].averageLine60 == 0)
                continue;
            temp.setX(getMarginLeft() + xstep * (i - m_beginDay) + 0.5 * m_lineWidth);
            temp.setY(getWidgetHeight() - (m_dataFile.kline[i].averageLine60 - m_lowestBid) * m_yScale - getMarginBottom());
            point.push_back(temp);
        }
        break;
    default:
        break;
    }

    QPainter painter(this);
    QPen pen;

    switch (day) {
    case 5:
        pen.setColor(Qt::white);
        break;
    case 10:
        pen.setColor(Qt::yellow);
        break;
    case 20:
        pen.setColor(Qt::magenta);
        break;
    case 30:
        pen.setColor(Qt::green);
        break;
    case 60:
        pen.setColor(Qt::cyan);
        break;
    default:
        pen.setColor(Qt::white);
        break;
    }
    painter.setPen(pen);
    QPolygon polykline(point);
    painter.drawPolyline(polykline);
}