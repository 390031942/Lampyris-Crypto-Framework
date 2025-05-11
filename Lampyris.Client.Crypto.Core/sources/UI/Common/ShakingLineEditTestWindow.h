#pragma once
#include "ShakingLineEdit.h"

class ShakingLineEditTestWindow : public QWidget {
    Q_OBJECT

public:
    explicit ShakingLineEditTestWindow(QWidget* parent = nullptr) : QWidget(parent) {
        QVBoxLayout* layout = new QVBoxLayout(this);

        // 创建自定义的 QLineEdit
        ShakingLineEdit* lineEdit = new ShakingLineEdit(this);
        lineEdit->setPlaceholderText("Enter text...");
        layout->addWidget(lineEdit);

        // 创建提交按钮
        QPushButton* submitButton = new QPushButton("Submit", this);
        layout->addWidget(submitButton);

        // 点击按钮时检查输入内容
        connect(submitButton, &QPushButton::clicked, this, [lineEdit]() {
            QString text = lineEdit->text();
            if (!isValidInput(text)) {
                lineEdit->triggerInvalidEffect(); // 触发震动和闪烁效果
            }
            });
    }

private:
    // 检查输入是否符合规则
    static bool isValidInput(const QString& text) {
        // 示例规则：输入内容必须是非空且长度大于 3
        return !text.isEmpty() && text.length() > 3;
    }
};
