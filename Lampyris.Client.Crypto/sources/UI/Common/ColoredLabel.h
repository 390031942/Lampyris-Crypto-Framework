#pragma once

// QT Include(s)
#include <QLabel>
#include <QColor>

/// <summary>
/// ColoredLabel 是一个支持设置文本颜色的 QLabel 子类。
/// 如果设置的颜色与当前颜色相同，则不会重复设置。
/// </summary>
class ColoredLabel : public QLabel {
    Q_OBJECT

public:
    explicit ColoredLabel(QWidget* parent = nullptr);
    void     setTextColor(const QColor& color);
    QColor   textColor() const;
private:
    QColor   m_currentColor; // 当前文本颜色
};