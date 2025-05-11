#pragma once
// QT Include(s)
#include <QApplication>
#include <QWidget>
#include <QPainter>
#include <QMouseEvent>
#include <QCursor>

/*
 * 自绘制Slider:
*/
class CustomSlider : public QWidget {
    Q_OBJECT

public:
    explicit CustomSlider(QWidget* parent = nullptr);
    // 设置 marker 数量
    void     setMarkerCount(int count);
    // 设置当前值
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

    int      m_markerCount; // marker 数量
    int      m_value;       // 当前值（百分比）
    bool     m_isDragging; // 是否正在拖动
    int      m_markerRadius = 5;
};