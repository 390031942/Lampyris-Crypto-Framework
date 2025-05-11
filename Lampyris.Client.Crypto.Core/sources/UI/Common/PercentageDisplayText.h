#pragma once

// Project Include(s)
#include <QApplication>
#include <QLabel>
#include <QVBoxLayout>
#include <QWidget>
#include <QPainter>
#include <QStyleOption>

/*
 * 自绘用于显示涨跌幅的控件，显示的文字如:xxx.xx%(保留2位有效数字),
 * 并且支持根据涨跌平的情况，让文字/背景颜色 显示为对应的颜色。
 */
class PercentageDisplayText : public QLabel {
    Q_OBJECT

public:
    enum DisplayMode {
        FontColorMode,  // 字体变色模式
        BackgroundColorMode // 背景变色模式
    };

    explicit    PercentageDisplayText(QWidget* parent = nullptr);
    void        setDisplayMode(DisplayMode displayMode);
    void        setValue(double newValue);
protected:
    void        paintEvent(QPaintEvent* event) override;
private:
    QColor      backgroundColor() const;
    QString     formattedText(int decimalPlaces = 2) const;
    void        updateAppearance();

    DisplayMode m_mode;
    double      m_value;

};
