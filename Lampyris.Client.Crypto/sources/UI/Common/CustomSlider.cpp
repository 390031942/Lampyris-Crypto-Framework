// Project Include(s)
#include "CustomSlider.h"
#include "GlobalUIStyle.h"

CustomSlider::CustomSlider(QWidget* parent)
    : QWidget(parent), m_markerCount(5), m_value(0), m_isDragging(false) {
    setMinimumSize(300, 50); // 固定控件大小
    setMouseTracking(true); // 开启鼠标跟踪
}

// 设置 marker 数量
void CustomSlider::setMarkerCount(int count) {
    m_markerCount = count;
    update();
}

// 设置当前值
void CustomSlider::setValue(int value) {
    m_value = value;
    update();
}

void CustomSlider::paintEvent(QPaintEvent* event) {
    Q_UNUSED(event);

    QPainter painter(this);
    painter.setRenderHint(QPainter::Antialiasing);

    // 黑色背景
    painter.fillRect(rect(), Qt::black);

    // 绘制水平线
    int lineY = height() / 2;
    int lineStartX = 20;
    int lineEndX = width() - 60;
    int step = 100 / (m_markerCount - 1);
    int markerSpacing = (lineEndX - lineStartX) / (m_markerCount - 1);

    // 绘制水平线的进度部分和未进度部分
    for (int i = 0; i < m_markerCount - 1; ++i) {
        int requiredValue = (i + 1) * step;
        int startX = lineStartX + i * markerSpacing + m_markerRadius;
        int endX = lineStartX + (i + 1) * markerSpacing - (m_markerRadius - 1);
        double progress = std::max(0.0, ((double)m_value - (requiredValue - step)) / (double)step);
        int progressEndX = startX + (endX - startX) * progress;
        if (progress > 0) {
            painter.setPen(QPen(GlobalUIStyle::orange)); // 橙色
            painter.drawLine(startX, lineY, progressEndX + 1, lineY);
        }
        painter.setPen(QPen(GlobalUIStyle::normal, 2)); 
        painter.drawLine(progressEndX + 1, lineY, endX, lineY);
    }

    // 绘制 marker（菱形）
    for (int i = 0; i < m_markerCount; ++i) {
        int markerX = lineStartX + i * markerSpacing;
        QPoint points[4] = {
            QPoint(markerX, lineY - m_markerRadius), // 上顶点
            QPoint(markerX - m_markerRadius, lineY), // 左顶点
            QPoint(markerX, lineY + m_markerRadius), // 下顶点
            QPoint(markerX + m_markerRadius, lineY)  // 右顶点
        };

        if (m_value >= (i * 100 / (m_markerCount - 1))) {
            painter.setBrush(GlobalUIStyle::orange); // 使用橙色填充
        }
        else {
            painter.setBrush(QBrush()); // 空心
        }

        painter.setPen(Qt::white); // 白色边框
        painter.drawPolygon(points, 4);
    }

    // 绘制进度百分比文本
    painter.setPen(GlobalUIStyle::normal);

    QFont font(QApplication::font());
    font.setPixelSize(12);
    painter.setFont(QFont(font));

    QString text = QString("%1%").arg(m_value);
    QFontMetrics fm(font);
    int textWidth = fm.horizontalAdvance(text);
    int textSpacing = (width() - lineEndX - textWidth) / 2;
    painter.drawText(lineEndX + textSpacing, lineY + 5, QString("%1%").arg(m_value));
}

void CustomSlider::enterEvent(QEnterEvent* event) {
    Q_UNUSED(event);
    // 鼠标进入控件时，设置为手掌样式
    setCursor(Qt::PointingHandCursor);
}

void CustomSlider::leaveEvent(QEvent* event) {
    Q_UNUSED(event);
    // 鼠标离开控件时，还原为箭头样式
    setCursor(Qt::ArrowCursor);
}

void CustomSlider::mousePressEvent(QMouseEvent* event) {
    if (event->button() == Qt::LeftButton) {
        m_isDragging = true; // 开始拖动
        updateValueFromMouse(event->pos().x());
    }
}

void CustomSlider::mouseMoveEvent(QMouseEvent* event) {
    if (m_isDragging) {
        updateValueFromMouse(event->pos().x());
    }
}

void CustomSlider::mouseReleaseEvent(QMouseEvent* event) {
    if (event->button() == Qt::LeftButton) {
        m_isDragging = false; // 停止拖动
    }
}

void CustomSlider::updateValueFromMouse(qreal mouseX) {
    int lineStartX = 20;
    int lineEndX = width() - 60;
    int markerSpacing = (lineEndX - lineStartX) / (m_markerCount - 1);

    // 分段计算 value
    for (int i = 0; i < m_markerCount; ++i) {
        int markerX = lineStartX + i * markerSpacing;

        // 如果鼠标在菱形范围内，直接设置为该 marker 的值
        if (mouseX >= markerX - 10 && mouseX <= markerX + 10) {
            setValue(i * 100 / (m_markerCount - 1));
            return;
        }

        // 如果鼠标在两个 marker 之间，计算过渡值
        if (i < m_markerCount - 1) {
            int nextMarkerX = lineStartX + (i + 1) * markerSpacing;
            if (mouseX > markerX + 10 && mouseX < nextMarkerX - 10) {
                qreal ratio = (mouseX - (markerX + 10)) / (nextMarkerX - markerX - 20);
                int value = (i * 100 / (m_markerCount - 1)) + ratio * (100 / (m_markerCount - 1));
                setValue(value);
                return;
            }
        }
    }
}
