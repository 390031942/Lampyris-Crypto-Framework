// Project Include(s)
#include "CustomSlider.h"
#include "GlobalUIStyle.h"

CustomSlider::CustomSlider(QWidget* parent)
    : QWidget(parent), m_markerCount(5), m_value(0), m_isDragging(false) {
    setMinimumSize(300, 50); // �̶��ؼ���С
    setMouseTracking(true); // ����������
}

// ���� marker ����
void CustomSlider::setMarkerCount(int count) {
    m_markerCount = count;
    update();
}

// ���õ�ǰֵ
void CustomSlider::setValue(int value) {
    m_value = value;
    update();
}

void CustomSlider::paintEvent(QPaintEvent* event) {
    Q_UNUSED(event);

    QPainter painter(this);
    painter.setRenderHint(QPainter::Antialiasing);

    // ��ɫ����
    painter.fillRect(rect(), Qt::black);

    // ����ˮƽ��
    int lineY = height() / 2;
    int lineStartX = 20;
    int lineEndX = width() - 60;
    int step = 100 / (m_markerCount - 1);
    int markerSpacing = (lineEndX - lineStartX) / (m_markerCount - 1);

    // ����ˮƽ�ߵĽ��Ȳ��ֺ�δ���Ȳ���
    for (int i = 0; i < m_markerCount - 1; ++i) {
        int requiredValue = (i + 1) * step;
        int startX = lineStartX + i * markerSpacing + m_markerRadius;
        int endX = lineStartX + (i + 1) * markerSpacing - (m_markerRadius - 1);
        double progress = std::max(0.0, ((double)m_value - (requiredValue - step)) / (double)step);
        int progressEndX = startX + (endX - startX) * progress;
        if (progress > 0) {
            painter.setPen(QPen(GlobalUIStyle::orange)); // ��ɫ
            painter.drawLine(startX, lineY, progressEndX + 1, lineY);
        }
        painter.setPen(QPen(GlobalUIStyle::normal, 2)); 
        painter.drawLine(progressEndX + 1, lineY, endX, lineY);
    }

    // ���� marker�����Σ�
    for (int i = 0; i < m_markerCount; ++i) {
        int markerX = lineStartX + i * markerSpacing;
        QPoint points[4] = {
            QPoint(markerX, lineY - m_markerRadius), // �϶���
            QPoint(markerX - m_markerRadius, lineY), // �󶥵�
            QPoint(markerX, lineY + m_markerRadius), // �¶���
            QPoint(markerX + m_markerRadius, lineY)  // �Ҷ���
        };

        if (m_value >= (i * 100 / (m_markerCount - 1))) {
            painter.setBrush(GlobalUIStyle::orange); // ʹ�ó�ɫ���
        }
        else {
            painter.setBrush(QBrush()); // ����
        }

        painter.setPen(Qt::white); // ��ɫ�߿�
        painter.drawPolygon(points, 4);
    }

    // ���ƽ��Ȱٷֱ��ı�
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
    // ������ؼ�ʱ������Ϊ������ʽ
    setCursor(Qt::PointingHandCursor);
}

void CustomSlider::leaveEvent(QEvent* event) {
    Q_UNUSED(event);
    // ����뿪�ؼ�ʱ����ԭΪ��ͷ��ʽ
    setCursor(Qt::ArrowCursor);
}

void CustomSlider::mousePressEvent(QMouseEvent* event) {
    if (event->button() == Qt::LeftButton) {
        m_isDragging = true; // ��ʼ�϶�
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
        m_isDragging = false; // ֹͣ�϶�
    }
}

void CustomSlider::updateValueFromMouse(qreal mouseX) {
    int lineStartX = 20;
    int lineEndX = width() - 60;
    int markerSpacing = (lineEndX - lineStartX) / (m_markerCount - 1);

    // �ֶμ��� value
    for (int i = 0; i < m_markerCount; ++i) {
        int markerX = lineStartX + i * markerSpacing;

        // �����������η�Χ�ڣ�ֱ������Ϊ�� marker ��ֵ
        if (mouseX >= markerX - 10 && mouseX <= markerX + 10) {
            setValue(i * 100 / (m_markerCount - 1));
            return;
        }

        // ������������ marker ֮�䣬�������ֵ
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
