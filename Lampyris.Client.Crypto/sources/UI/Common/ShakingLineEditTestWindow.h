#pragma once
#include "ShakingLineEdit.h"

class ShakingLineEditTestWindow : public QWidget {
    Q_OBJECT

public:
    explicit ShakingLineEditTestWindow(QWidget* parent = nullptr) : QWidget(parent) {
        QVBoxLayout* layout = new QVBoxLayout(this);

        // �����Զ���� QLineEdit
        ShakingLineEdit* lineEdit = new ShakingLineEdit(this);
        lineEdit->setPlaceholderText("Enter text...");
        layout->addWidget(lineEdit);

        // �����ύ��ť
        QPushButton* submitButton = new QPushButton("Submit", this);
        layout->addWidget(submitButton);

        // �����ťʱ�����������
        connect(submitButton, &QPushButton::clicked, this, [lineEdit]() {
            QString text = lineEdit->text();
            if (!isValidInput(text)) {
                lineEdit->triggerInvalidEffect(); // �����𶯺���˸Ч��
            }
            });
    }

private:
    // ��������Ƿ���Ϲ���
    static bool isValidInput(const QString& text) {
        // ʾ�������������ݱ����Ƿǿ��ҳ��ȴ��� 3
        return !text.isEmpty() && text.length() > 3;
    }
};
