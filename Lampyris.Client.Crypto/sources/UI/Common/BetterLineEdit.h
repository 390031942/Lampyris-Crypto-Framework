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
 * ���ư�QLineEdit
 * 1) ֧��Enter����������������� ��ȷ���������
 * 2) ֧������ʱ�·���ʾ����ʴ���
 * 3) ֧��ĳЩ����(���������ݲ��Ϸ�)ʱ�����𶯺���˸Ч��
 */
class BetterLineEdit : public QLineEdit {
    Q_OBJECT
    Q_PROPERTY(QColor backgroundColor READ backgroundColor WRITE setBackgroundColor)

public:
    explicit BetterLineEdit(QWidget* parent = nullptr);

    // ������ʷ��¼����ʴ���
    void setHistoryWidget(SymbolSearchResultWidget* historyWidget) {
        m_historyWidget = historyWidget;
    }

    // �����𶯺���˸Ч��
    void triggerInvalidEffect() {
        shake();        // ������
        flashRed();     // ��������Ϊ����ɫ����˸
    }

    // ���ñ�����ɫ
    void setBackgroundColor(const QColor& color) {
        QPalette p = palette();
        p.setColor(QPalette::Base, color);
        setPalette(p);
    }

    // ��ȡ������ɫ
    QColor backgroundColor() const {
        return palette().color(QPalette::Base);
    }

    // ����ѡ���б�
    void setOptions(const QStringList& options) {
        if (options.isEmpty())
            return;

        this->m_dropDownWidget->setOptions(options);
        this->m_optionsButton->show();
        this->m_line->show(); // Ĭ������
    }

    void clearOptions() {
        this->m_dropDownWidget->setOptions({});
        this->m_optionsButton->hide();
        this->m_line->hide(); // Ĭ������
    }

    void setOptionalText(const QString& text) {
        this->m_optionalText->setText(text + "  ");
    }
Q_SIGNALS:
    void signalOutFocus();
    void signalInFocus();
    void optionSelected(const QString& option); // ѡ��ѡ���ź�
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
    QToolButton* m_optionsButton; // �Ҳఴť
    QLabel* m_optionalText;
    bool m_focus; // ����״̬
    SymbolSearchResultWidget* m_historyWidget; // ��ʷ��¼����ʴ���
    DropDownWidget* m_dropDownWidget;
};
