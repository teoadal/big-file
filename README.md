# BigFile (External sort)

Это консольная утилита для создания, сортировки и проверки больших файлов.
Для удобства использования, к BigFile подключена библиотека CommandLineParser, чтобы превращать аргументы командной строки в объектное представление.
Утилита использует алгоритм External Sort ([Внешняя сортировка](https://ru.wikipedia.org/wiki/%D0%92%D0%BD%D0%B5%D1%88%D0%BD%D1%8F%D1%8F_%D1%81%D0%BE%D1%80%D1%82%D0%B8%D1%80%D0%BE%D0%B2%D0%BA%D0%B0)).

## Входные данные

Утилита предназначена для сортировки файлов, которые содержат данные в следующем формате - [long].[whitespace][string][new_line]. Пример данных в исходном файле:
```txt
-332112. Test test test
11. 
1233. Rest rest rest
1. Test test test
123. Rest rest rest 
```
Задача утилиты отсортировать данные по строке (максимальный размер 1024 символа), а затем, по номеру. Результатом сортировки примера будет:
```txt
11. 
-332112. Ast ast ast
1. Ast ast ast
123. Rest rest rest
1233. Rest rest rest
```

## Команда --help

Вы можете вызвать встроенный help с описанием функционала утилиты.

Например, вызов `BigFile --help` вернёт набор команд, существующих в утилите.

Вызов `BigFile create --help` покажет, какие аргументы существуют у команды `create` и какие значения используются по-умолчанию.

## Создание файла с тестовыми данными

Необходимо открыть командную строку в месте расположения файла `BigFile.exe` и написать `BigFile create`. 
Эта команда создаст файл `randomData.txt` с размером в 10 ГБ + повторяющиеся блоки текста.

### Создание файла с хорошим распределением тестовых данных

Создание файла с настройками по-умолчанию - быстрая операция, но в ней не достаточно часто используется Random, что порождает множество примерно одинаковых строк в файле. Чтобы осуществить создание файла с хорошим случайным распределением, необходимо вызвать `BigFile create --well`. 

Обратите внимание, что это значительно медленнее подхода по-умолчанию, однако подготавливает более качественные данные.

### Создание файла необходимого размера

Создание файла необходимого размера осуществляется с ключём `--size`, после которого нужно указать размер файла в байтах.
Например, `BigFile create --well -size 107374182400` создаст тестовый файл размером в 100ГБ + повторяющиеся блоки текста.

## Сортировка файла

Сортировка созданного файла `randomData.txt` осуществляется с помощью команды `BigFile sort`.
Результатом выполнения операции будет файла `sortedData.txt`.

Для случая сортировки больших файлов в общем случае используется алгоритм External Sort, который предполагает наличие на диске места, необходимого для расположения частей исходного файла. 
Если размер исходного файла 10 ГБ, то необходимо не менее 10 ГБ свободного места. 

Сортировка осуществляется в два этапа: разделение (создание отсортированных частей исходнго файла) и слияние (объединение частей в результирующий файл).

Разные тестовые данные формируют разный размер отсортированных частей исходного файла. Это связано с тем, что в утилите есть буффер определенного размера. Как только он заполняется, осуществляется его сортировка и сбрасывание на диск.

## Тестирование результатов сортировки

Тестирование отсортированного файла `sortedData.txt` осуществляется с помощью команды `BigFile test`. 
Основная задача тестирования - проверить, что все данные в файле находятся в упорядоченном виде.
Тестирование включает подсчёт строк, определение максимального и минимального номера.

Для проверки, что в отсортированном файле существуют все строки из исходного файла, необходимо вызвать `BigFile test --compare`.


