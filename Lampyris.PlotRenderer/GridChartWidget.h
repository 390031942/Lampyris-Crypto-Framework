#pragma once
// QT Include(s)
#include <QWidget>
#include <QPainter>

// Project Include(s)
#include "QuoteCandleDataView.h"

class GridChartWidget : public QWidget {
    Q_OBJECT
public:
    explicit GridChartWidget(QWidget* parent = 0);

    virtual void initialize();
    virtual void drawBackground();
    virtual void calculateCellGridHeight();
    virtual void calculateCellGridWidth();

    void drawGrid();
    void drawHorizontalLines();
    void drawVerticalLines();

    int getMarginLeft() const { return marginLeft; }
    void setMarginLeft(int value) { marginLeft = value; }

    int getMarginRight() const { return marginRight; }
    void setMarginRight(int value) { marginRight = value; }

    int getMarginTop() const { return marginTop; }
    void setMarginTop(int value) { marginTop = value; }

    int getMarginBottom() const { return marginBottom; }
    void setMarginBottom(int value) { marginBottom = value; }

    int getWidgetHeight() const { return widgetHeight; }
    int getWidgetWidth() const { return widgetWidth; }

    double getGridHeight() const { return gridHeight; }
    double getGridWidth() const { return gridWidth; }

    int getHorizontalGridCount() const { return horizontalGridCount; }
    void setHorizontalGridCount(int value) { horizontalGridCount = value; }

    int getVerticalGridCount() const { return verticalGridCount; }
    void setVerticalGridCount(int value) { verticalGridCount = value; }

    double getCellGridHeightMin() const { return cellGridHeightMin; }
    void setCellGridHeightMin(double value) { cellGridHeightMin = value; }

    double getCellGridWidthMin() const { return cellGridWidthMin; }
    void setCellGridWidthMin(double value) { cellGridWidthMin = value; }

    double getCellGridHeight() const { return cellGridHeight; }
    double getCellGridWidth() const { return cellGridWidth; }

    void setDataView(QuoteCandleDataView* dataView) { m_dataView = dataView; }
    QuoteCandleDataView* getDataView(QuoteCandleDataView* dataView) const { return m_dataView; }
protected:
    virtual void resizeEvent(QResizeEvent* event);
    virtual void paintEvent(QPaintEvent* event);
    virtual void mouseMoveEvent(QMouseEvent* event) override;
    virtual void mousePressEvent(QMouseEvent* event) override;

    bool isMouseHovered() const;

    void drawMouseMoveCrossVerLine();
    void drawMouseMoveCrossHorLine();

    // 鼠标位置
    QPoint   m_mousePoint;

    // 是否显示十字线
    bool     m_bCross = false;

    // 键盘是否按下
    bool     m_isKeyDown = false;

    // 表格距边框距离
    int      marginLeft = 3;
    int      marginRight = 20;
    int      marginTop = 3;
    int      marginBottom = 3;
private:
    // 当前 widget 的宽度和高度
    int widgetHeight;
    int widgetWidth;

    // 当前表格的宽度和高度
    double gridHeight;
    double gridWidth;

    // 当前表格中最小单元格的宽度和高度
    double cellGridHeight;
    double cellGridWidth;

    // 表格中单元格的数量
    int horizontalGridCount;
    int verticalGridCount;

    // 当前表格中最小单元格的宽度和高度的最小值
    double cellGridHeightMin;
    double cellGridWidthMin;

    QColor mouseCrossColor = QColor(255, 255, 255);
    QColor backgroundColor = QColor(24,26,32);
    QColor gridColor = QColor(80,83,90);

    QuoteCandleDataView* m_dataView;
};