#pragma once

#include <QWidget>
#include <QVector>
#include <QPointF>
#include <QTimer>

class AssetTrendCurveWidget : public QWidget {
    Q_OBJECT

public:
    explicit AssetTrendCurveWidget(QWidget* parent = nullptr);

protected:
    void paintEvent(QPaintEvent* event) override;
    void mousePressEvent(QMouseEvent* event) override;
    void mouseReleaseEvent(QMouseEvent* event) override;
    void mouseMoveEvent(QMouseEvent* event) override;
    void enterEvent(QEnterEvent* event) override;
    void leaveEvent(QEvent* event) override;
    void resizeEvent(QResizeEvent* event) override;

private:
    QVector<QPointF> m_dataPoints; // 数据点
    QPointF m_highlightedPoint;   // 高亮的数据点
    bool m_isPointHighlighted;    // 是否有高亮点
    QString m_tipText;            // 提示框的文字内容
    QTimer* m_longPressTimer;     // 长按计时器
    QPointF m_pressPosition;      // 鼠标按下的位置

    qreal m_xScale;               // X轴缩放比例
    qreal m_yScale;               // Y轴缩放比例
    QPointF m_offset;             // 偏移量

    int m_marginTop;              // 图像顶部边距
    int m_marginBottom;           // 图像底部边距
    int m_marginLeft;             // 图像左侧边距
    int m_marginRight;            // 图像右侧边距
    int m_tipOffset;              // 提示框的 X 偏移量

    void onLongPress();
    void generateRandomData();    // 随机生成数据点
    void findClosestPoint(const QPointF& pos); // 查找最近的数据点
    void drawTips(QPainter& painter, const QPointF& point); // 绘制提示框
    void calculateScaling();      // 计算缩放比例和偏移量
};