   M S B A M L   `   `   `  ����   CAGA*    $CAGA.Dialogue.ListPlainOptionsWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  Choose an option��$	��360��$	��480�� � WindowStartupLocation$ CenterScreen=� � SizeToContent$ WidthAndHeight=� � WindowStyle$ 
ToolWindow=� � 	Resources    HoveredItemBrush �� EndPoint �� 
StartPoint � Color � Offset  OptionListTemplate	 �� DataType
 ��Name �� Padding �� CornerRadius �� Triggers �� Value �� Binding �� RelativeSource �� Mode �� AncestorType �� Path �� 
TargetName �� Value �� Property��%�  &	        &	 |     7   7   �� $ 0.5,1&�$ 0.5,0&�.��� $ Lavender��$
 0.15Q�� $ 	AliceBlue��$
 0.85Q�� $ 	CadetBlue��$ 1Q��� $	 Whatever��.R��� $
 	TheBorder��
�������$��1q�$ 10q�$ 4��$��5q�._��� .+��� $	��350��$��13(�$
��Bold"����� *Title+�� $	��350������ *Description+���� $ 0.5,1&�$ 0.5,0&�.��� $ 	AliceBlue��$
 0.15Q�� $ White��$
 0.85Q�� $ Lavender��$ 1Q� �� $
 True�� ��  �� $ FindAncestor=�" ��$ IsMouseOver�.a��� $ 	TheBorder��8    � ���� $
 True�� �� *
IsSelected+.a��� $ 	TheBorder��8   � ��.��� $��Center=�$��Center=�$��10q� � RowDefinitions �� $��* ��� $��* �.+��� -   $��	HeadingTB��$��0��$��Heading��$��5q� �� Padding$ 5q�$��30(�$
��Bold"�$
��Wrap=��� -   $��OptionLB��$��1��#��[ 
������ 
������    NoFocusVisualStyle �� 
TargetType �� Color��%=  &	       '�� %     7  (�� Z     * SystemColors.HighlightBrushKey+)(�� q     *SystemColors.ControlBrushKey+)(�� �     *$"SystemColors.HighlightTextBrushKey+)�� " ��.���  � ��
 ���� �� " ��.���  �@�FocusVisualStyle8   -   �� $ Transparent���� $ Transparent���� $ Black��