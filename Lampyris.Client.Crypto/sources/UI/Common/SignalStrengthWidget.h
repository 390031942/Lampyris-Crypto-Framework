#pragma once
#include <QWidget>
#include <QPainter>
#include <QTimer>
#include <QLabel>
#include <QVBoxLayout>
#include <QHBoxLayout>
#include <QProcess>
#include <QRegularExpression>
#include <QApplication>

class SignalStrengthWidget : public QWidget {
    Q_OBJECT

public:
    explicit SignalStrengthWidget(QWidget* parent = nullptr)
        : QWidget(parent), m_signalStrength(0), m_delayMs(0), m_spacingBetweenBarsAndText(10) {
        setFixedWidth(80);
        m_timer = new QTimer(this);
        connect(m_timer, &QTimer::timeout, this, &SignalStrengthWidget::updateDelay);
        m_timer->start(1000); // ÿ��ˢ��һ��
    }

    // �����ӳ�ֵ��ˢ���źŸ�
    void updateDelay() {
        // ʹ�� ping �����ӳ�
        QProcess pingProcess;
        pingProcess.start("ping", QStringList() << "-n" << "1" << "www.baidu.com");
        pingProcess.waitForFinished();

        QString output = QString::fromLocal8Bit(pingProcess.readAllStandardOutput());
        QRegularExpression regex(QString::fromLocal8Bit("ʱ��=([0-9]+)ms"));
        QRegularExpressionMatch match = regex.match(output);

        if (match.hasMatch()) {
            m_delayMs = match.captured(1).toInt();
        }
        else {
            m_delayMs = 9999; // ����޷���ȡ�ӳ٣�����Ϊ���ֵ
        }

        // �����ӳ�ֵ�����ź�ǿ��
        if (m_delayMs <= 50) {
            m_signalStrength = 4;
        }
        else if (m_delayMs <= 100) {
            m_signalStrength = 3;
        }
        else if (m_delayMs <= 300) {
            m_signalStrength = 2;
        }
        else {
            m_signalStrength = 1;
        }

        update(); // �����ػ�
    }

protected:
    void paintEvent(QPaintEvent* event) override {
        Q_UNUSED(event);

        QPainter painter(this);
        painter.setRenderHint(QPainter::Antialiasing);

        // ��ȡ�ؼ��Ŀ�Ⱥ͸߶�
        int width = this->width();
        int height = this->height();

        // �źŸ�Ŀ�ȡ��߶Ⱥͼ��
        int barWidth = 4;
        int spacing = 2; // ����֮��ļ��
        int totalBarsWidth = 4 * barWidth + 3 * spacing; // �����źŸ���ܿ��

        // �ӳ��ı��Ŀ�ȣ����㣩
        QString delayText = QString("%1ms").arg(m_delayMs);
        QFont font = QApplication::font();
        QFontMetrics fontMetrics(font);
        int textWidth = fontMetrics.horizontalAdvance(delayText); // �ı����
        int textHeight = fontMetrics.height();                   // �ı��߶�

        // �����źŸ���ı������������ܿ��
        int totalWidth = totalBarsWidth + m_spacingBetweenBarsAndText + textWidth;
        int totalHeight = qMax(14, textHeight); // ȡ�źŸ���ı������߶�

        // ��������������ʼλ�ã�ʹ���������
        int startX = (width - totalWidth) / 2; // ˮƽ����
        int startY = (height - totalHeight) / 2; // ��ֱ����

        // �����ź�ǿ��������ɫ
        QColor activeColor;
        if (m_signalStrength == 4) {
            activeColor = QColor(82, 189, 135); // ��ɫ
        }
        else if (m_signalStrength == 1) {
            activeColor = QColor(242, 80, 95); // ��ɫ
        }
        else {
            activeColor = QColor(255, 204, 0); // ��ɫ
        }

        // �����źŸ�
        int baseY = startY + totalHeight; // �źŸ�ĵײ�λ��
        for (int i = 1; i <= 4; ++i) {
            int rectHeight = 5 + 3 * (i - 1); // ÿ���źŸ�ĸ߶�������
            int rectX = startX + (i - 1) * (barWidth + spacing); // ÿ���źŸ�� X �����𽥵���
            int rectY = baseY - rectHeight; // �ӵײ���ʼ����

            QRect rect(rectX, rectY, barWidth, rectHeight);

            if (i <= m_signalStrength) {
                painter.setBrush(activeColor); // ��ǰ�źŸ����ɫ
            }
            else {
                painter.setBrush(QColor(200, 200, 200)); // ��ɫ��δ����ĸ��ӣ�
            }

            painter.setPen(Qt::NoPen);
            painter.drawRect(rect);
        }

        // �����ӳ�ֵ
        painter.setPen(Qt::white);
        painter.setFont(font);

        // �ı�����ʼλ�ã����źŸ���Ҳࣩ
        int textX = startX + totalBarsWidth + m_spacingBetweenBarsAndText;
        int textY = startY + (totalHeight - textHeight) / 2; // ��ֱ����

        painter.drawText(textX, textY + textHeight - fontMetrics.descent(), delayText);
    }

private:
    int m_signalStrength; // �ź�ǿ�ȣ�1 �� 4��
    int m_delayMs;        // ��ǰ�ӳٺ���ֵ
    QTimer* m_timer;      // ��ʱ������ˢ���ӳ�
    int m_spacingBetweenBarsAndText; // �źŸ���ı�֮��ļ��
};
