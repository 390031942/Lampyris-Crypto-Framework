#pragma once
// QT Include(s)
#include <QApplication>
#include <QWidget>
#include <QPainter>
#include <QMouseEvent>
#include <QCursor>

/*
 * �Ի���Slider:
*/
class CustomSlider : public QWidget {
    Q_OBJECT

public:
    explicit CustomSlider(QWidget* parent = nullptr);
    // ���� marker ����
    void     setMarkerCount(int count);
    // ���õ�ǰֵ
    void     setValue(int value);
protected:
    void     paintEvent(QPaintEvent* event) override;
    void     enterEvent(QEnterEvent* event) override;
    void     leaveEvent(QEvent* event) override;
    void     mousePressEvent(QMouseEvent* event) override;
    void     mouseMoveEvent(QMouseEvent* event) override;
    void     mouseReleaseEvent(QMouseEvent* event) override;
private:     
    void     updateValueFromMouse(qreal mouseX);

    int      m_markerCount; // marker ����
    int      m_value;       // ��ǰֵ���ٷֱȣ�
    bool     m_isDragging; // �Ƿ������϶�
    int      m_markerRadius = 5;
};