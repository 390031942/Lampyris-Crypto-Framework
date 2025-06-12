#pragma once

// QT Include(s)
#include <QLineEdit>
#include <QFocusEvent>
#include <QKeyEvent>
#include <QPropertyAnimation>
#include <QPalette>
#include <QPoint>
#include <QToolButton>

// Project Include(s)
#include "UI/Standalong/Common/DropDownWidget.h"

class SymbolSearchResultWidget;

/*
 * 定制版QLineEdit
 * 1) 支持Enter键或鼠标点击其它区域 来确认输入完成
 * 2) 支持输入时下方显示联想词窗口
 * 3) 支持某些场合(如输入内容不合法)时触发震动和闪烁效果
 */
class BetterLineEdit : public QLineEdit {
    Q_OBJECT
    Q_PROPERTY(QColor backgroundColor READ backgroundColor WRITE setBackgroundColor)

public:
    explicit BetterLineEdit(QWidget* parent = nullptr);

    // 设置历史记录联想词窗口
    void setHistoryWidget(SymbolSearchResultWidget* historyWidget) {
        m_historyWidget = historyWidget;
    }

    // 触发震动和闪烁效果
    void triggerInvalidEffect() {
        shake();        // 左右震动
        flashRed();     // 背景渐变为淡红色并闪烁
    }

    // 设置背景颜色
    void setBackgroundColor(const QColor& color) {
        QPalette p = palette();
        p.setColor(QPalette::Base, color);
        setPalette(p);
    }

    // 获取背景颜色
    QColor backgroundColor() const {
        return palette().color(QPalette::Base);
    }

    // 设置选项列表
    void setOptions(const QStringList& options) {
        if (options.isEmpty())
            return;

        this->m_dropDownWidget->setOptions(options);
        this->m_optionsButton->show();
        this->m_line->show(); // 默认隐藏
    }

    void clearOptions() {
        this->m_dropDownWidget->setOptions({});
        this->m_optionsButton->hide();
        this->m_line->hide(); // 默认隐藏
    }

    void setOptionalText(const QString& text) {
        this->m_optionalText->setText(text + "  ");
    }
Q_SIGNALS:
    void signalOutFocus();
    void signalInFocus();
    void optionSelected(const QString& option); // 选项选择信号
protected:
    void focusOutEvent(QFocusEvent* event) override;
    void focusInEvent(QFocusEvent* event) override;
    void keyPressEvent(QKeyEvent* event) override;
    bool eventFilter(QObject* obj, QEvent* event) override;
private:
    void shake();
    void flashRed();
private:
    QFrame* m_line;
    QToolButton* m_optionsButton; // 右侧按钮
    QLabel* m_optionalText;
    bool m_focus; // 焦点状态
    SymbolSearchResultWidget* m_historyWidget; // 历史记录联想词窗口
    DropDownWidget* m_dropDownWidget;
};
