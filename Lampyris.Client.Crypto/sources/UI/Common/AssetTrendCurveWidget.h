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
    QVector<QPointF> m_dataPoints; // ���ݵ�
    QPointF m_highlightedPoint;   // ���������ݵ�
    bool m_isPointHighlighted;    // �Ƿ��и�����
    QString m_tipText;            // ��ʾ�����������
    QTimer* m_longPressTimer;     // ������ʱ��
    QPointF m_pressPosition;      // ��갴�µ�λ��

    qreal m_xScale;               // X�����ű���
    qreal m_yScale;               // Y�����ű���
    QPointF m_offset;             // ƫ����

    int m_marginTop;              // ͼ�񶥲��߾�
    int m_marginBottom;           // ͼ��ײ��߾�
    int m_marginLeft;             // ͼ�����߾�
    int m_marginRight;            // ͼ���Ҳ�߾�
    int m_tipOffset;              // ��ʾ��� X ƫ����

    void onLongPress();
    void generateRandomData();    // ����������ݵ�
    void findClosestPoint(const QPointF& pos); // ������������ݵ�
    void drawTips(QPainter& painter, const QPointF& point); // ������ʾ��
    void calculateScaling();      // �������ű�����ƫ����
};