#pragma once
// QT Include(s)
#include <QApplication>
#include <QWidget>
#include <QHBoxLayout>
#include <QVBoxLayout>
#include <QLineEdit>
#include <QPushButton>
#include <QLabel>
#include <QSpacerItem>
#include <QPropertyAnimation>
#include <QFocusEvent>
#include <QTimer>
#include <QDebug>
#include <QScrollBar>

class SymbolSearchResultWidget : public QWidget {
    Q_OBJECT
public:
    SymbolSearchResultWidget(QWidget* parent) : QWidget(parent) {
        setWindowFlags(Qt::Tool | Qt::FramelessWindowHint);
        setAttribute(Qt::WA_StyledBackground);
        setStyleSheet("background-color: #1e2329; border: 1px solid gray; border-radius: 3px;");
        // setFocusPolicy(Qt::NoFocus);

        layout = new QVBoxLayout(this);
        layout->setContentsMargins(5, 5, 5, 5);
        layout->setSpacing(5);
    }

    void setHistory(const QStringList& history, int maxWidth) {
        // �����ʷ��¼
        QLayoutItem* item;
        while ((item = layout->takeAt(0)) != nullptr) {
            delete item->widget();
            delete item;
        }
        // �����ʷ��¼��ť
        QHBoxLayout* rowLayout = new QHBoxLayout();
        int currentRowWidth = 0;

        for (const QString& record : history) {
            QPushButton* button = new QPushButton(record, this);
            button->setStyleSheet("background-color: #f0f0f0; border: none; padding: 5px;");
            button->setSizePolicy(QSizePolicy::Fixed, QSizePolicy::Fixed);

            int buttonWidth = button->sizeHint().width();
            currentRowWidth += buttonWidth;

            if (currentRowWidth > maxWidth) {
                // ����
                layout->addLayout(rowLayout);
                rowLayout = new QHBoxLayout();
                currentRowWidth = buttonWidth;
            }

            rowLayout->addWidget(button);

            // ���Ӱ�ť����¼�
            connect(button, &QPushButton::clicked, this, [record]() {
                qDebug() << "Clicked history record:" << record;
            });
        }

        // ������һ��
        layout->addLayout(rowLayout);
    }

protected:
    void mousePressEvent(QMouseEvent* event) override {
        QWidget::mousePressEvent(event);
    }
private:
    QVBoxLayout* layout;
    QScrollBar*  scrollBar;
};