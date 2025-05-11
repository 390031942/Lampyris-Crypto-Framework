#include "AssetTrendCurveWidget.h"
#include <QPainter>
#include <QMouseEvent>
#include <QFontMetrics>
#include <QRandomGenerator>
#include <QDebug>

AssetTrendCurveWidget::AssetTrendCurveWidget(QWidget* parent)
    : QWidget(parent), m_isPointHighlighted(false),
    m_marginTop(20), m_marginBottom(20), m_marginLeft(20), m_marginRight(20), m_tipOffset(10) {
    // ��ʼ��������ʱ��
    m_longPressTimer = new QTimer(this);
    m_longPressTimer->setSingleShot(true);
    m_longPressTimer->setInterval(500); // ����ʱ��Ϊ 500 ����

    // ����������ݵ�
    generateRandomData();

    // �������ű�����ƫ����
    calculateScaling();
}

void AssetTrendCurveWidget::generateRandomData() {
    m_dataPoints.clear();
    int numPoints = 50; // �������50�����ݵ�
    for (int i = 0; i < numPoints; ++i) {
        qreal x = i;
        qreal y = QRandomGenerator::global()->bounded(0, 500);  // Y�᷶Χ
        m_dataPoints.append(QPointF(x, y));
    }
}

void AssetTrendCurveWidget::calculateScaling() {
    if (m_dataPoints.isEmpty()) return;

    // ��ȡ���ݵ�ķ�Χ
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

    // �������ű���
    m_xScale = (width() - m_marginLeft - m_marginRight) / (maxX - minX);
    m_yScale = (height() - m_marginTop - m_marginBottom) / (maxY - minY);

    // ����ƫ����
    m_offset = QPointF(-minX * m_xScale + m_marginLeft, -minY * m_yScale + m_marginTop);
}

void AssetTrendCurveWidget::drawTips(QPainter& painter, const QPointF& point) {
    // ������ʾ����ʽ
    QFont font = painter.font();
    font.setPointSize(10);
    painter.setFont(font);

    QFontMetrics metrics(font);
    int textWidth = metrics.horizontalAdvance(m_tipText);
    int textHeight = metrics.height() * 2; // ��������

    // ��ʾ��λ��
    QRectF tipRect;
    if (point.x() < width() / 2) {
        // ��ǰ���ڴ������࣬��ʾ����ʾ���Ҳ�
        tipRect = QRectF(point.x() + m_tipOffset, point.y() - textHeight / 2,
            textWidth + 10, textHeight + 10);
    }
    else {
        // ��ǰ���ڴ����Ұ�࣬��ʾ����ʾ�����
        tipRect = QRectF(point.x() - m_tipOffset - textWidth - 10, point.y() - textHeight / 2,
            textWidth + 10, textHeight + 10);
    }

    // ������ʾ��λ�ã�ȷ�����봰�ڱ߽��ཻ
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

    // ����Բ�Ǿ��α���
    painter.setBrush(Qt::darkGray);
    painter.setPen(Qt::NoPen);
    painter.drawRoundedRect(tipRect, 5, 5);

    // ��������
    painter.setPen(Qt::white);
    painter.drawText(tipRect, Qt::AlignCenter, m_tipText);
}

void AssetTrendCurveWidget::paintEvent(QPaintEvent* event) {
    Q_UNUSED(event);

    QPainter painter(this);
    painter.setRenderHint(QPainter::Antialiasing);

    // ���Ʊ���
    painter.fillRect(rect(), Qt::black);

    // ��������
    QPen pen(Qt::white, 2);
    painter.setPen(pen);
    for (int i = 0; i < m_dataPoints.size() - 1; ++i) {
        QPointF p1(m_dataPoints[i].x() * m_xScale + m_offset.x(),
            m_dataPoints[i].y() * m_yScale + m_offset.y());
        QPointF p2(m_dataPoints[i + 1].x() * m_xScale + m_offset.x(),
            m_dataPoints[i + 1].y() * m_yScale + m_offset.y());
        painter.drawLine(p1, p2);
    }

    // ���Ƴ�ɫ���
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

    // ���Ƹ�����
    if (m_isPointHighlighted) {
        QPointF highlightPoint(m_highlightedPoint.x() * m_xScale + m_offset.x(),
            m_highlightedPoint.y() * m_yScale + m_offset.y());
        painter.setBrush(Qt::white);
        painter.setPen(Qt::NoPen);
        painter.drawEllipse(highlightPoint, 5, 5); // ����ԲȦ

        // ������ʾ��
        drawTips(painter, highlightPoint);
    }
}

void AssetTrendCurveWidget::mousePressEvent(QMouseEvent* event) {
    m_pressPosition = event->pos();
    m_longPressTimer->start(); // ����������ʱ��
}

void AssetTrendCurveWidget::mouseReleaseEvent(QMouseEvent* event) {
    Q_UNUSED(event);
    m_longPressTimer->stop(); // ֹͣ������ʱ��
}

void AssetTrendCurveWidget::mouseMoveEvent(QMouseEvent* event) {
    m_pressPosition = event->pos(); // ���°���λ��
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
    findClosestPoint(m_pressPosition); // ������������ݵ�
    update(); // �����ػ�
}

void AssetTrendCurveWidget::findClosestPoint(const QPointF& pos) {
    if (m_dataPoints.isEmpty()) return;

    // ���������λ����������ݵ�
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

    // ���¸�����
    m_highlightedPoint = closestPoint;
    m_isPointHighlighted = true;

    // ������ʾ������
    m_tipText = QString("X: %1\nY: %2").arg(closestPoint.x()).arg(closestPoint.y());
}
