// Project Include(s)
#include "GridChartWidget.h"

// QT Include(s)
#include <QPainter>
#include <QPen>
#include <QMouseEvent>

GridChartWidget::GridChartWidget(QWidget* parent)
    : QWidget(parent) {
    initialize();
}

void GridChartWidget::drawBackground() {
    this->setAutoFillBackground(true);
    QPalette palette;
    palette.setColor(QPalette::Window, backgroundColor);
    this->setPalette(palette);
}

void GridChartWidget::resizeEvent(QResizeEvent* event) {
    widgetWidth = this->width();
    widgetHeight = this->height();
    gridHeight = widgetHeight - marginTop - marginBottom;
    gridWidth = widgetWidth - marginLeft - marginRight;
    calculateCellGridHeight();
    calculateCellGridWidth();
}

void GridChartWidget::initialize() {
    cellGridHeight = 60;
    cellGridHeightMin = 60;
    cellGridWidth = 640;
    cellGridWidthMin = 640;
    drawBackground();
}

void GridChartWidget::calculateCellGridHeight() {
    horizontalGridCount = 0;
    int height = gridHeight;
    while (height - cellGridHeightMin > cellGridHeightMin) {
        ++horizontalGridCount;
        height -= cellGridHeightMin;
    }
    cellGridHeight = gridHeight / horizontalGridCount;
}

void GridChartWidget::calculateCellGridWidth() {
    verticalGridCount = 0;
    int width = gridWidth;
    while (width - cellGridWidthMin > cellGridWidthMin) {
        ++verticalGridCount;
        width -= cellGridWidthMin;
    }
    cellGridWidth = gridWidth / verticalGridCount;
}

void GridChartWidget::paintEvent(QPaintEvent* event) {
    drawGrid();
}

void GridChartWidget::mouseMoveEvent(QMouseEvent* event) {
    m_mousePoint = event->pos();
    m_isKeyDown = false;
    update();
}


void GridChartWidget::mousePressEvent(QMouseEvent* event) {
    if (event->button() == Qt::LeftButton) {
        m_bCross = !m_bCross;
        update();
    }
}

bool GridChartWidget::isMouseHovered() const {
    if (m_mousePoint.x() < getMarginLeft() || m_mousePoint.x() > getWidgetWidth() - getMarginRight())
        return false;

    if (m_mousePoint.y() < getMarginTop() || m_mousePoint.y() > getWidgetHeight() - getMarginBottom())
        return false;

    return true;
}

void GridChartWidget::drawMouseMoveCrossVerLine() {
    if (!isMouseHovered())
        return;

    QPainter painter(this);
    QPen pen;
    pen.setColor(mouseCrossColor);
    pen.setWidth(1);
    painter.setPen(pen);
    painter.drawLine(m_mousePoint.x(), getMarginTop(), m_mousePoint.x(), getWidgetHeight() - getMarginBottom());
}

void GridChartWidget::drawMouseMoveCrossHorLine() {
    if (!isMouseHovered())
        return;

    QPainter painter(this);
    QPen pen;
    pen.setColor(mouseCrossColor);
    pen.setWidth(1);
    painter.setPen(pen);

    painter.drawLine(getMarginLeft(), m_mousePoint.y(),
        getWidgetWidth() - getMarginRight(), m_mousePoint.y());
}

void GridChartWidget::drawHorizontalLines() {
    QPainter painter(this);
    QPen pen;
    pen.setColor(gridColor);
    painter.setPen(pen);

    int xStart = marginLeft;
    int yStart = marginTop;
    int xEnd = widgetWidth - marginRight;
    int yEnd = marginTop;

    for (int i = 0; i < horizontalGridCount + 1; ++i) {
        if (i == 0 || i == horizontalGridCount) {
            pen.setStyle(Qt::SolidLine);
            painter.setPen(pen);
        }
        else {
            pen.setStyle(Qt::DashDotLine);
            painter.setPen(pen);
        }
        painter.drawLine(xStart, yStart + i * cellGridHeight,
            xEnd, yEnd + i * cellGridHeight);
    }

    if (0 == horizontalGridCount) {
        painter.drawLine(marginLeft, marginTop,
            widgetWidth - marginRight, marginTop);
        painter.drawLine(marginLeft, marginTop + gridHeight,
            widgetWidth - marginRight, marginTop + gridHeight);
    }
}

void GridChartWidget::drawVerticalLines() {
    QPainter painter(this);
    QPen pen;
    pen.setColor(gridColor);
    painter.setPen(pen);

    int xStart = marginLeft;
    int yStart = marginTop;
    int xEnd = marginLeft;
    int yEnd = widgetHeight - marginBottom;

    for (int i = 0; i < verticalGridCount + 1; ++i) {
        if (i == 0 || i == verticalGridCount) {
            pen.setStyle(Qt::SolidLine);
            painter.setPen(pen);
        }
        else {
            pen.setStyle(Qt::DashDotLine);
            painter.setPen(pen);
        }
        painter.drawLine(xStart + i * cellGridWidth, yStart,
            xEnd + i * cellGridWidth, yEnd);
    }

    if (verticalGridCount == 0) {
        painter.drawLine(marginLeft, marginTop,
            marginLeft, widgetHeight - marginBottom);
        painter.drawLine(marginLeft + gridWidth, marginTop,
            marginLeft + gridWidth, marginTop + gridHeight);
    }
}

void GridChartWidget::drawGrid() {
    drawHorizontalLines();
    drawVerticalLines();
}