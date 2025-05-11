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
        m_timer->start(1000); // 每秒刷新一次
    }

    // 更新延迟值并刷新信号格
    void updateDelay() {
        // 使用 ping 测试延迟
        QProcess pingProcess;
        pingProcess.start("ping", QStringList() << "-n" << "1" << "www.baidu.com");
        pingProcess.waitForFinished();

        QString output = QString::fromLocal8Bit(pingProcess.readAllStandardOutput());
        QRegularExpression regex(QString::fromLocal8Bit("时间=([0-9]+)ms"));
        QRegularExpressionMatch match = regex.match(output);

        if (match.hasMatch()) {
            m_delayMs = match.captured(1).toInt();
        }
        else {
            m_delayMs = 9999; // 如果无法获取延迟，设置为最大值
        }

        // 根据延迟值更新信号强度
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

        update(); // 触发重绘
    }

protected:
    void paintEvent(QPaintEvent* event) override {
        Q_UNUSED(event);

        QPainter painter(this);
        painter.setRenderHint(QPainter::Antialiasing);

        // 获取控件的宽度和高度
        int width = this->width();
        int height = this->height();

        // 信号格的宽度、高度和间隔
        int barWidth = 4;
        int spacing = 2; // 矩形之间的间隔
        int totalBarsWidth = 4 * barWidth + 3 * spacing; // 所有信号格的总宽度

        // 延迟文本的宽度（估算）
        QString delayText = QString("%1ms").arg(m_delayMs);
        QFont font = QApplication::font();
        QFontMetrics fontMetrics(font);
        int textWidth = fontMetrics.horizontalAdvance(delayText); // 文本宽度
        int textHeight = fontMetrics.height();                   // 文本高度

        // 计算信号格和文本的组合区域的总宽度
        int totalWidth = totalBarsWidth + m_spacingBetweenBarsAndText + textWidth;
        int totalHeight = qMax(14, textHeight); // 取信号格和文本的最大高度

        // 计算组合区域的起始位置，使其整体居中
        int startX = (width - totalWidth) / 2; // 水平居中
        int startY = (height - totalHeight) / 2; // 垂直居中

        // 根据信号强度设置颜色
        QColor activeColor;
        if (m_signalStrength == 4) {
            activeColor = QColor(82, 189, 135); // 绿色
        }
        else if (m_signalStrength == 1) {
            activeColor = QColor(242, 80, 95); // 红色
        }
        else {
            activeColor = QColor(255, 204, 0); // 黄色
        }

        // 绘制信号格
        int baseY = startY + totalHeight; // 信号格的底部位置
        for (int i = 1; i <= 4; ++i) {
            int rectHeight = 5 + 3 * (i - 1); // 每个信号格的高度逐渐增加
            int rectX = startX + (i - 1) * (barWidth + spacing); // 每个信号格的 X 坐标逐渐递增
            int rectY = baseY - rectHeight; // 从底部开始绘制

            QRect rect(rectX, rectY, barWidth, rectHeight);

            if (i <= m_signalStrength) {
                painter.setBrush(activeColor); // 当前信号格的颜色
            }
            else {
                painter.setBrush(QColor(200, 200, 200)); // 灰色（未激活的格子）
            }

            painter.setPen(Qt::NoPen);
            painter.drawRect(rect);
        }

        // 绘制延迟值
        painter.setPen(Qt::white);
        painter.setFont(font);

        // 文本的起始位置（在信号格的右侧）
        int textX = startX + totalBarsWidth + m_spacingBetweenBarsAndText;
        int textY = startY + (totalHeight - textHeight) / 2; // 垂直居中

        painter.drawText(textX, textY + textHeight - fontMetrics.descent(), delayText);
    }

private:
    int m_signalStrength; // 信号强度（1 到 4）
    int m_delayMs;        // 当前延迟毫秒值
    QTimer* m_timer;      // 定时器用于刷新延迟
    int m_spacingBetweenBarsAndText; // 信号格和文本之间的间距
};
