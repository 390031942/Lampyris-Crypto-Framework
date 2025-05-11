#include "AssetTrendCurveWidget.h"
#include <QPainter>
#include <QMouseEvent>
#include <QFontMetrics>
#include <QRandomGenerator>
#include <QDebug>

AssetTrendCurveWidget::AssetTrendCurveWidget(QWidget* parent)
    : QWidget(parent), m_isPointHighlighted(false),
    m_marginTop(20), m_marginBottom(20), m_marginLeft(20), m_marginRight(20), m_tipOffset(10) {
    // 初始化长按计时器
    m_longPressTimer = new QTimer(this);
    m_longPressTimer->setSingleShot(true);
    m_longPressTimer->setInterval(500); // 长按时间为 500 毫秒

    // 随机生成数据点
    generateRandomData();

    // 计算缩放比例和偏移量
    calculateScaling();
}

void AssetTrendCurveWidget::generateRandomData() {
    m_dataPoints.clear();
    int numPoints = 50; // 随机生成50个数据点
    for (int i = 0; i < numPoints; ++i) {
        qreal x = i;
        qreal y = QRandomGenerator::global()->bounded(0, 500);  // Y轴范围
        m_dataPoints.append(QPointF(x, y));
    }
}

void AssetTrendCurveWidget::calculateScaling() {
    if (m_dataPoints.isEmpty()) return;

    // 获取数据点的范围
    qreal minX = m_dataPoints[0].x();
    qreal maxX = m_dataPoints[0].x();
    qreal minY = m_dataPoints[0].y();
    qreal maxY = m_dataPoints[0].y();

    for (const QPointF& point : m_dataPoints) {
        minX = qMin(minX, point.x());
        maxX = qMax(maxX, point.x());
        minY = qMin(minY, point.y());
        maxY = qMax(maxY, point.y());
    }

    // 计算缩放比例
    m_xScale = (width() - m_marginLeft - m_marginRight) / (maxX - minX);
    m_yScale = (height() - m_marginTop - m_marginBottom) / (maxY - minY);

    // 计算偏移量
    m_offset = QPointF(-minX * m_xScale + m_marginLeft, -minY * m_yScale + m_marginTop);
}

void AssetTrendCurveWidget::drawTips(QPainter& painter, const QPointF& point) {
    // 设置提示框样式
    QFont font = painter.font();
    font.setPointSize(10);
    painter.setFont(font);

    QFontMetrics metrics(font);
    int textWidth = metrics.horizontalAdvance(m_tipText);
    int textHeight = metrics.height() * 2; // 两行文字

    // 提示框位置
    QRectF tipRect;
    if (point.x() < width() / 2) {
        // 当前点在窗口左半侧，提示框显示在右侧
        tipRect = QRectF(point.x() + m_tipOffset, point.y() - textHeight / 2,
            textWidth + 10, textHeight + 10);
    }
    else {
        // 当前点在窗口右半侧，提示框显示在左侧
        tipRect = QRectF(point.x() - m_tipOffset - textWidth - 10, point.y() - textHeight / 2,
            textWidth + 10, textHeight + 10);
    }

    // 修正提示框位置，确保不与窗口边界相交
    if (tipRect.left() < m_marginLeft) {
        tipRect.moveLeft(m_marginLeft);
    }
    if (tipRect.right() > width() - m_marginRight) {
        tipRect.moveRight(width() - m_marginRight);
    }
    if (tipRect.top() < m_marginTop) {
        tipRect.moveTop(m_marginTop);
    }
    if (tipRect.bottom() > height() - m_marginBottom) {
        tipRect.moveBottom(height() - m_marginBottom);
    }

    // 绘制圆角矩形背景
    painter.setBrush(Qt::darkGray);
    painter.setPen(Qt::NoPen);
    painter.drawRoundedRect(tipRect, 5, 5);

    // 绘制文字
    painter.setPen(Qt::white);
    painter.drawText(tipRect, Qt::AlignCenter, m_tipText);
}

void AssetTrendCurveWidget::paintEvent(QPaintEvent* event) {
    Q_UNUSED(event);

    QPainter painter(this);
    painter.setRenderHint(QPainter::Antialiasing);

    // 绘制背景
    painter.fillRect(rect(), Qt::black);

    // 绘制折线
    QPen pen(Qt::white, 2);
    painter.setPen(pen);
    for (int i = 0; i < m_dataPoints.size() - 1; ++i) {
        QPointF p1(m_dataPoints[i].x() * m_xScale + m_offset.x(),
            m_dataPoints[i].y() * m_yScale + m_offset.y());
        QPointF p2(m_dataPoints[i + 1].x() * m_xScale + m_offset.x(),
            m_dataPoints[i + 1].y() * m_yScale + m_offset.y());
        painter.drawLine(p1, p2);
    }

    // 绘制橙色描边
    pen.setColor(QColor(221,133,29));
    pen.setWidth(4);
    painter.setPen(pen);
    for (int i = 0; i < m_dataPoints.size() - 1; ++i) {
        QPointF p1(m_dataPoints[i].x() * m_xScale + m_offset.x(),
            m_dataPoints[i].y() * m_yScale + m_offset.y());
        QPointF p2(m_dataPoints[i + 1].x() * m_xScale + m_offset.x(),
            m_dataPoints[i + 1].y() * m_yScale + m_offset.y());
        painter.drawLine(p1, p2);
    }

    // 绘制高亮点
    if (m_isPointHighlighted) {
        QPointF highlightPoint(m_highlightedPoint.x() * m_xScale + m_offset.x(),
            m_highlightedPoint.y() * m_yScale + m_offset.y());
        painter.setBrush(Qt::white);
        painter.setPen(Qt::NoPen);
        painter.drawEllipse(highlightPoint, 5, 5); // 绘制圆圈

        // 绘制提示框
        drawTips(painter, highlightPoint);
    }
}

void AssetTrendCurveWidget::mousePressEvent(QMouseEvent* event) {
    m_pressPosition = event->pos();
    m_longPressTimer->start(); // 启动长按计时器
}

void AssetTrendCurveWidget::mouseReleaseEvent(QMouseEvent* event) {
    Q_UNUSED(event);
    m_longPressTimer->stop(); // 停止长按计时器
}

void AssetTrendCurveWidget::mouseMoveEvent(QMouseEvent* event) {
    m_pressPosition = event->pos(); // 更新按下位置
    onLongPress();
}

void AssetTrendCurveWidget::enterEvent(QEnterEvent* event) {
    QWidget::enterEvent(event);
}

void AssetTrendCurveWidget::leaveEvent(QEvent* event) {
    QWidget::leaveEvent(event);
    m_isPointHighlighted = false;
}

void AssetTrendCurveWidget::resizeEvent(QResizeEvent* event) {
    QWidget::resizeEvent(event);
    calculateScaling();
}

void AssetTrendCurveWidget::onLongPress() {
    findClosestPoint(m_pressPosition); // 查找最近的数据点
    update(); // 触发重绘
}

void AssetTrendCurveWidget::findClosestPoint(const QPointF& pos) {
    if (m_dataPoints.isEmpty()) return;

    // 查找离鼠标位置最近的数据点
    qreal minDistance = std::numeric_limits<qreal>::max();
    QPointF closestPoint;
    for (const QPointF& point : m_dataPoints) {
        QPointF scaledPoint = QPointF(point.x() * m_xScale, point.y() * m_yScale) + m_offset;
        qreal distance = std::abs(scaledPoint.x() - pos.x());
        if (distance < minDistance) {
            minDistance = distance;
            closestPoint = point;
        }
    }

    // 更新高亮点
    m_highlightedPoint = closestPoint;
    m_isPointHighlighted = true;

    // 更新提示框文字
    m_tipText = QString("X: %1\nY: %2").arg(closestPoint.x()).arg(closestPoint.y());
}
