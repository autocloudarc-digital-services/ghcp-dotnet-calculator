import pytest

from calculator import add, divide, multiply, subtract


@pytest.mark.parametrize(
    ("operation", "first_number", "second_number", "expected"),
    [
        (add, 2, 3, 5),
        (subtract, 8, 3, 5),
        (multiply, 4, 3, 12),
        (divide, 9, 3, 3),
        (divide, 5, 2, 2.5),
    ],
)
def test_arithmetic_operations_return_expected_results(
    operation, first_number, second_number, expected
):
    assert operation(first_number, second_number) == expected


def test_divide_by_zero_raises_zero_division_error():
    with pytest.raises(ZeroDivisionError):
        divide(1, 0)
